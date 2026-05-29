using FluentAssertions;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests;

public class InfrastructureTests
{
    [Fact]
    public void SqliteFixture_CanCreateAndDisposeContext()
    {
        using var fixture = new SqliteInMemoryFixture();
        var options = fixture.CreateOptions();
        using var ctx = new AppDbContext(options);

        ctx.Should().NotBeNull();
        ctx.AlertasHistorico.Should().NotBeNull();
        ctx.ZonasReferencia.Should().NotBeNull();
    }

    [Fact]
    public void SqliteFixture_EnsureCreated_TablesExist()
    {
        using var fixture = new SqliteInMemoryFixture();
        var options = fixture.CreateOptions();
        using var ctx = new AppDbContext(options);

        ctx.Database.CanConnect().Should().BeTrue();
    }
}
