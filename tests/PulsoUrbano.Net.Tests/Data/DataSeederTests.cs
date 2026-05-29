using FluentAssertions;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Data;

public class DataSeederTests
{
    [Fact]
    public async Task SeedAsync_FreshDb_Inserts5ZonasAndAtLeast40Alertas()
    {
        using var fixture = new SqliteInMemoryFixture();
        var options = fixture.CreateOptions();
        await using var ctx = new AppDbContext(options);

        await DataSeeder.SeedAsync(ctx);

        ctx.ZonasReferencia.Count().Should().Be(5);
        ctx.AlertasHistorico.Count().Should().BeGreaterThanOrEqualTo(40);
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicate()
    {
        using var fixture = new SqliteInMemoryFixture();
        var options = fixture.CreateOptions();
        await using var ctx = new AppDbContext(options);

        await DataSeeder.SeedAsync(ctx);
        var zonaCount = ctx.ZonasReferencia.Count();
        var alertaCount = ctx.AlertasHistorico.Count();

        await DataSeeder.SeedAsync(ctx); // second call must be no-op

        ctx.ZonasReferencia.Count().Should().Be(zonaCount);
        ctx.AlertasHistorico.Count().Should().Be(alertaCount);
    }
}
