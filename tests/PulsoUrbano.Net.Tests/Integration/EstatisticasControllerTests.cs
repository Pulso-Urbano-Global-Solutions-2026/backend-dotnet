using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Integration;

[Trait("Category", "Integration")]
public class EstatisticasControllerTests : IClassFixture<PulsoWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    public EstatisticasControllerTests(PulsoWebAppFactory factory)
        => _client = factory.CreateClient();

    private async Task<int> GetFirstZonaId()
    {
        var res  = await _client.GetAsync("/api/alertas?dias=30&tamanhoPagina=1");
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("dados")[0].GetProperty("zonaId").GetInt32();
    }

    [Fact]
    public async Task GetZona_Returns200_WithCorrectTotalsForSeededData()
    {
        var zonaId = await GetFirstZonaId();
        var res    = await _client.GetAsync($"/api/estatisticas/zona/{zonaId}?dias=30");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(), _json);

        body.GetProperty("zonaId").GetInt32().Should().Be(zonaId);
        body.GetProperty("totalAlertas").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("scoreMedia").GetDouble().Should().BeInRange(0, 100);
        body.GetProperty("diasComAlerta").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("tendencia").GetString()
            .Should().BeOneOf("MELHORANDO", "PIORANDO", "ESTAVEL");

        // alertasPorNivel should be a non-empty object
        var niveis = body.GetProperty("alertasPorNivel");
        niveis.EnumerateObject().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetZona_UnknownZona_Returns404()
    {
        var res = await _client.GetAsync("/api/estatisticas/zona/99999");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetResumo_ZonaComMaisAlertas_IsCentro()
    {
        var res = await _client.GetAsync("/api/estatisticas/resumo");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(), _json);

        body.GetProperty("totalZonas").GetInt32().Should().Be(5);
        body.GetProperty("totalAlertas30dias").GetInt32().Should().BeGreaterThanOrEqualTo(40);

        // Seeder gives Centro 12 alertas (most) — it must be the top zone
        var zona = body.GetProperty("zonaComMaisAlertas");
        zona.GetProperty("nome").GetString().Should().Be("Centro");
        zona.GetProperty("total").GetInt32().Should().BeGreaterThan(0);

        body.GetProperty("nivelPredominante").GetString()
            .Should().BeOneOf("ATENCAO", "ALERTA", "EMERGENCIA");
    }
}
