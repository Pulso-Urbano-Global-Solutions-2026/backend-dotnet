using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace PulsoUrbano.Net.Tests.Infrastructure;

[Trait("Category", "Integration")]
public class PulsoWebAppFactoryTests : IClassFixture<PulsoWebAppFactory>
{
    private readonly HttpClient _client;

    public PulsoWebAppFactoryTests(PulsoWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Factory_Boots_HealthReturns200()
    {
        var res = await _client.GetAsync("/api/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Factory_PublicGet_AlertasReturns200()
    {
        var res = await _client.GetAsync("/api/alertas");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Factory_ProtectedPost_WithoutToken_Returns401()
    {
        var res = await _client.PostAsJsonAsync("/api/alertas", new { });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
