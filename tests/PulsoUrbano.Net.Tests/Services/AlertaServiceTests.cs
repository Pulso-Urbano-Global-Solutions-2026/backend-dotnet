using FluentAssertions;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Services;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Services;

public class AlertaServiceTests
{
    private static async Task<(AppDbContext ctx, AlertaService svc)> BuildSeeded()
    {
        var fixture = new SqliteInMemoryFixture();
        var opts    = fixture.CreateOptions();
        var ctx     = new AppDbContext(opts);
        await DataSeeder.SeedAsync(ctx);
        return (ctx, new AlertaService(ctx));
    }

    [Fact]
    public async Task CreateAsync_ValidDto_PersistsAndReturnsDtoWithZonaNome()
    {
        var (ctx, svc) = await BuildSeeded();
        var zonaId = ctx.ZonasReferencia.First().Id;

        var dto = new AlertaCreateDTO(zonaId, "ALERTA", 55.0, 30.0, "Teste de criação.");
        var result = await svc.CreateAsync(dto);

        result.Id.Should().BeGreaterThan(0);
        result.ZonaNome.Should().NotBeNullOrEmpty();
        result.NivelAlerta.Should().Be("ALERTA");
        result.Confirmado.Should().BeFalse();
        ctx.AlertasHistorico.Count().Should().BeGreaterThan(44); // seeder + 1
    }

    [Fact]
    public async Task CreateAsync_InvalidZonaId_ThrowsKeyNotFound()
    {
        var (_, svc) = await BuildSeeded();
        var act = () => svc.CreateAsync(new AlertaCreateDTO(9999, "ALERTA", 55.0, 30.0, "X"));
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsKeyNotFound()
    {
        var (_, svc) = await BuildSeeded();
        var act = () => svc.GetByIdAsync(99999);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsDto()
    {
        var (ctx, svc) = await BuildSeeded();
        var existingId = ctx.AlertasHistorico.First().Id;

        var result = await svc.GetByIdAsync(existingId);
        result.Id.Should().Be(existingId);
        result.ZonaNome.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAsync_FiltersByZonaAndDias_AndPaginates()
    {
        var (ctx, svc) = await BuildSeeded();
        var zonaId = ctx.ZonasReferencia.First().Id; // Centro

        var page1 = await svc.GetAsync(zonaId, dias: 30, pagina: 1, tamanhoPagina: 5);

        page1.Total.Should().BeGreaterThan(0);
        page1.Dados.Should().HaveCountLessOrEqualTo(5);
        page1.Dados.Should().AllSatisfy(d => d.ZonaId.Should().Be(zonaId));
    }

    [Fact]
    public async Task GetAsync_NoFilter_ReturnsAllWithin30Days()
    {
        var (_, svc) = await BuildSeeded();
        var result = await svc.GetAsync(null, dias: 30, pagina: 1, tamanhoPagina: 100);

        result.Total.Should().BeGreaterThanOrEqualTo(40);
        result.Pagina.Should().Be(1);
        result.TamanhoPagina.Should().Be(100);
    }

    [Fact]
    public async Task ConfirmarAsync_SetsConfirmadoTrue()
    {
        var (ctx, svc) = await BuildSeeded();
        var alerta = ctx.AlertasHistorico.First(a => !a.Confirmado);

        var result = await svc.ConfirmarAsync(alerta.Id, true);
        result.Confirmado.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecord_SubsequentGetThrows()
    {
        var (ctx, svc) = await BuildSeeded();
        var id = ctx.AlertasHistorico.First().Id;

        await svc.DeleteAsync(id);

        var act = () => svc.GetByIdAsync(id);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
