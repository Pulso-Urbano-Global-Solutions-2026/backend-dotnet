using FluentAssertions;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.Entities;
using PulsoUrbano.Net.Services;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Services;

public class EstatisticasServiceTests
{
    private static async Task<(AppDbContext ctx, EstatisticasService svc)> BuildSeeded()
    {
        var fixture = new SqliteInMemoryFixture();
        var ctx     = new AppDbContext(fixture.CreateOptions());
        await DataSeeder.SeedAsync(ctx);
        return (ctx, new EstatisticasService(ctx));
    }

    [Fact]
    public async Task GetByZonaAsync_KnownZona_ReturnsCorrectCountsAndAvg()
    {
        var (ctx, svc) = await BuildSeeded();
        var zona = ctx.ZonasReferencia.First(); // Centro

        var result = await svc.GetByZonaAsync(zona.Id, dias: 30);

        result.ZonaId.Should().Be(zona.Id);
        result.ZonaNome.Should().Be(zona.Nome);
        result.TotalAlertas.Should().BeGreaterThan(0);
        result.AlertasPorNivel.Should().NotBeEmpty();
        result.ScoreMedia.Should().BeInRange(0, 100);
        result.Tendencia.Should().BeOneOf("MELHORANDO", "PIORANDO", "ESTAVEL");
    }

    [Fact]
    public async Task GetByZonaAsync_UnknownZona_ThrowsKeyNotFound()
    {
        var (_, svc) = await BuildSeeded();
        var act = () => svc.GetByZonaAsync(99999);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByZonaAsync_Tendencia_ImprovingWindow_ReturnsMELHORANDO()
    {
        // Arrange: fresh context with controlled data
        var fixture = new SqliteInMemoryFixture();
        var ctx     = new AppDbContext(fixture.CreateOptions());

        var zona = new ZonaReferencia { Nome = "Zona Teste", Municipio = "São Paulo" };
        ctx.ZonasReferencia.Add(zona);
        await ctx.SaveChangesAsync();

        var now = DateTime.UtcNow;
        // First half (16–30 days ago): low scores → avg ~30
        for (int i = 16; i <= 30; i++)
            ctx.AlertasHistorico.Add(new AlertaHistorico
            {
                ZonaId = zona.Id, NivelAlerta = "EMERGENCIA",
                ScoreRegistrado = 30, No2Registrado = 40,
                TextoRecomendacao = "ruim", DtAlerta = now.AddDays(-i)
            });

        // Second half (0–15 days ago): high scores → avg ~80
        for (int i = 0; i <= 15; i++)
            ctx.AlertasHistorico.Add(new AlertaHistorico
            {
                ZonaId = zona.Id, NivelAlerta = "ATENCAO",
                ScoreRegistrado = 80, No2Registrado = 20,
                TextoRecomendacao = "ok", DtAlerta = now.AddDays(-i)
            });

        await ctx.SaveChangesAsync();
        var svc = new EstatisticasService(ctx);

        var result = await svc.GetByZonaAsync(zona.Id, dias: 30);

        result.Tendencia.Should().Be("MELHORANDO");
    }

    [Fact]
    public async Task GetByZonaAsync_Tendencia_WorseningWindow_ReturnsPIORANDO()
    {
        var fixture = new SqliteInMemoryFixture();
        var ctx     = new AppDbContext(fixture.CreateOptions());

        var zona = new ZonaReferencia { Nome = "Zona Piora", Municipio = "São Paulo" };
        ctx.ZonasReferencia.Add(zona);
        await ctx.SaveChangesAsync();

        var now = DateTime.UtcNow;
        // First half: high scores
        for (int i = 16; i <= 30; i++)
            ctx.AlertasHistorico.Add(new AlertaHistorico
            {
                ZonaId = zona.Id, NivelAlerta = "ATENCAO",
                ScoreRegistrado = 80, No2Registrado = 20,
                TextoRecomendacao = "ok", DtAlerta = now.AddDays(-i)
            });

        // Second half: low scores
        for (int i = 0; i <= 15; i++)
            ctx.AlertasHistorico.Add(new AlertaHistorico
            {
                ZonaId = zona.Id, NivelAlerta = "EMERGENCIA",
                ScoreRegistrado = 30, No2Registrado = 45,
                TextoRecomendacao = "ruim", DtAlerta = now.AddDays(-i)
            });

        await ctx.SaveChangesAsync();
        var svc = new EstatisticasService(ctx);

        var result = await svc.GetByZonaAsync(zona.Id, dias: 30);

        result.Tendencia.Should().Be("PIORANDO");
    }

    [Fact]
    public async Task GetResumoGeralAsync_PicksZonaWithMostAlertas()
    {
        var (ctx, svc) = await BuildSeeded();

        var result = await svc.GetResumoGeralAsync();

        result.TotalZonas.Should().Be(5);
        result.TotalAlertas30dias.Should().BeGreaterThanOrEqualTo(40);
        result.ZonaComMaisAlertas.Nome.Should().Be("Centro"); // seeder: Centro has 12 alertas
        result.ZonaComMaisAlertas.Total.Should().BeGreaterThan(0);
        result.NivelPredominante.Should().BeOneOf("ATENCAO", "ALERTA", "EMERGENCIA");
        result.DtAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
