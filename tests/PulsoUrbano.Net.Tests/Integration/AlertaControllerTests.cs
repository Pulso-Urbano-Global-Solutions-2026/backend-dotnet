using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Integration;

[Trait("Category", "Integration")]
public class AlertaControllerTests : IClassFixture<PulsoWebAppFactory>
{
    private readonly PulsoWebAppFactory _factory;
    private readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    public AlertaControllerTests(PulsoWebAppFactory factory)
        => _factory = factory;

    private async Task<int> GetFirstZonaId()
    {
        var client = _factory.CreateClient();
        var res    = await client.GetAsync("/api/alertas?dias=30&tamanhoPagina=1");
        var body   = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("dados")[0].GetProperty("zonaId").GetInt32();
    }

    // ── N-30 test 1 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_ValidAlerta_Returns201_WithLocation_AndRetrievable()
    {
        var zonaId = await GetFirstZonaId();
        var client = _factory.CreateAuthenticatedClient();

        var payload = new AlertaCreateDTO(
            ZonaId:            zonaId,
            NivelAlerta:       "ALERTA",
            ScoreRegistrado:   38.5,
            No2Registrado:     41.2,
            TextoRecomendacao: "Evite esforço físico ao ar livre.");

        // POST → 201
        var postRes = await client.PostAsJsonAsync("/api/alertas", payload);
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);
        postRes.Headers.Location.Should().NotBeNull("POST deve retornar Location header");

        // GET from Location → 200 with matching body
        var getRes  = await client.GetAsync(postRes.Headers.Location);
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await getRes.Content.ReadFromJsonAsync<AlertaResponseDTO>();
        created!.NivelAlerta.Should().Be("ALERTA");
        created.ScoreRegistrado.Should().Be(38.5);
        created.ZonaId.Should().Be(zonaId);
        created.Confirmado.Should().BeFalse();
    }

    // ── N-30 test 2 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_UnknownId_Returns404_WithErrorEnvelope()
    {
        var client = _factory.CreateClient();

        var res  = await client.GetAsync("/api/alertas/99999");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = JsonSerializer.Deserialize<ErrorResponseDTO>(
            await res.Content.ReadAsStringAsync(), _json);

        body.Should().NotBeNull();
        body!.Status.Should().Be(404);
        body.Erro.Should().NotBeNullOrEmpty();
    }

    // ── N-30 test 3 ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_WithZonaFilter_ReturnsPaginatedResults()
    {
        var zonaId = await GetFirstZonaId();
        var client = _factory.CreateClient();

        var res  = await client.GetAsync($"/api/alertas?zonaId={zonaId}&dias=30&pagina=1&tamanhoPagina=5");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = JsonSerializer.Deserialize<JsonElement>(
            await res.Content.ReadAsStringAsync(), _json);

        body.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("pagina").GetInt32().Should().Be(1);
        body.GetProperty("tamanhoPagina").GetInt32().Should().Be(5);

        var dados = body.GetProperty("dados");
        dados.GetArrayLength().Should().BeGreaterThan(0);

        // All items belong to requested zone
        foreach (var item in dados.EnumerateArray())
            item.GetProperty("zonaId").GetInt32().Should().Be(zonaId);
    }

    // ── N-31 tests ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_Confirmar_SetsConfirmadoTrue()
    {
        var zonaId = await GetFirstZonaId();
        var client = _factory.CreateAuthenticatedClient();

        // Create a fresh alerta to confirm (avoid depending on seeded state)
        var postRes = await client.PostAsJsonAsync("/api/alertas",
            new AlertaCreateDTO(zonaId, "ATENCAO", 70.0, 22.0, "Teste confirmar."));
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postRes.Content.ReadFromJsonAsync<AlertaResponseDTO>();
        created!.Confirmado.Should().BeFalse();

        // PUT confirmar
        var putRes = await client.PutAsJsonAsync(
            $"/api/alertas/{created.Id}/confirmar",
            new AlertaConfirmarDTO(true));
        putRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await putRes.Content.ReadFromJsonAsync<AlertaResponseDTO>();
        updated!.Confirmado.Should().BeTrue();
        updated.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Delete_Then_GetById_Returns404()
    {
        var zonaId = await GetFirstZonaId();
        var client = _factory.CreateAuthenticatedClient();

        // Create then immediately delete
        var postRes = await client.PostAsJsonAsync("/api/alertas",
            new AlertaCreateDTO(zonaId, "EMERGENCIA", 25.0, 50.0, "Teste delete."));
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await postRes.Content.ReadFromJsonAsync<AlertaResponseDTO>();

        var delRes = await client.DeleteAsync($"/api/alertas/{created!.Id}");
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Subsequent GET must 404
        var getRes = await client.GetAsync($"/api/alertas/{created.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient(); // no auth header
        var res = await client.PostAsJsonAsync("/api/alertas",
            new AlertaCreateDTO(1, "ALERTA", 50.0, 30.0, "Sem token."));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_WithoutToken_Returns200()
    {
        var client = _factory.CreateClient(); // no auth header — reads are public
        var res = await client.GetAsync("/api/alertas");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
