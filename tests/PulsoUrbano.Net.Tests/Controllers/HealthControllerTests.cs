using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PulsoUrbano.Net.Controllers;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public async Task Get_ReturnsHealthy_WithConnectedDatabase()
    {
        using var fixture = new SqliteInMemoryFixture();
        await using var ctx = new AppDbContext(fixture.CreateOptions());
        var controller = new HealthController(ctx);

        var result = await controller.Get();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<HealthResponseDTO>().Subject;

        body.Status.Should().Be("healthy");
        body.Servico.Should().Be("pulso-urbano-dotnet");
        body.Database.Should().Be("connected");
        body.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
