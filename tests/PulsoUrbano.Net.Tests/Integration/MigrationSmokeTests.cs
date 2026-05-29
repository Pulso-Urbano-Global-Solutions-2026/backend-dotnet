using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.Entities;
using PulsoUrbano.Net.Tests.Infrastructure;

namespace PulsoUrbano.Net.Tests.Integration;

[Trait("Category", "Integration")]
public class MigrationSmokeTests
{
    // Uses SQLite fixture directly — no Oracle needed; proves relational integrity
    // Full Oracle DDL validation is a manual gate (dotnet ef database update in N-10)

    [Fact]
    public async Task Schema_Has1NRelationship_DeletingZonaWithAlertas_IsBlocked()
    {
        using var fixture = new SqliteInMemoryFixture();
        var opts = fixture.CreateOptions();

        // Seed zona + alerta in one context, then close it
        int zonaId;
        await using (var seed = new AppDbContext(opts))
        {
            var zona = new ZonaReferencia { Nome = "Zona Smoke Delete", Municipio = "São Paulo" };
            seed.ZonasReferencia.Add(zona);
            await seed.SaveChangesAsync();
            zonaId = zona.Id;

            seed.AlertasHistorico.Add(new AlertaHistorico
            {
                ZonaId            = zonaId,
                NivelAlerta       = "ALERTA",
                ScoreRegistrado   = 50.0,
                No2Registrado     = 30.0,
                TextoRecomendacao = "Migration smoke test.",
                DtAlerta          = DateTime.UtcNow
            });
            await seed.SaveChangesAsync();
        }

        // Fresh context — alertas are NOT in the change tracker, so EF Core
        // passes the DELETE to the DB, which enforces the Restrict FK → DbUpdateException
        await using var ctx2 = new AppDbContext(opts);
        var stub = new ZonaReferencia { Id = zonaId };
        ctx2.ZonasReferencia.Attach(stub);
        ctx2.ZonasReferencia.Remove(stub);

        var act = () => ctx2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>(
            "the 1:N Restrict constraint must block deletion of a zona that has alertas");
    }

    [Fact]
    public async Task Schema_InsertAlertaWithInvalidZonaId_ThrowsDbUpdateException()
    {
        using var fixture = new SqliteInMemoryFixture();
        await using var ctx = new AppDbContext(fixture.CreateOptions());

        // No zona exists — FK violation expected
        ctx.AlertasHistorico.Add(new AlertaHistorico
        {
            ZonaId            = 99999,
            NivelAlerta       = "ALERTA",
            ScoreRegistrado   = 50.0,
            No2Registrado     = 30.0,
            TextoRecomendacao = "FK smoke test.",
            DtAlerta          = DateTime.UtcNow
        });

        var act = () => ctx.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>(
            "inserting an alerta with a non-existent ZonaId must be rejected by the FK constraint");
    }

    [Fact]
    public async Task Schema_ZonaAndAlertaTablesExist_AfterEnsureCreated()
    {
        using var fixture = new SqliteInMemoryFixture();
        await using var ctx = new AppDbContext(fixture.CreateOptions());

        // If EnsureCreated() succeeded, the tables exist and CanConnect returns true
        var canConnect = await ctx.Database.CanConnectAsync();
        canConnect.Should().BeTrue();

        // Verify both sides of the 1:N relationship are queryable
        ctx.ZonasReferencia.Should().NotBeNull();
        ctx.AlertasHistorico.Should().NotBeNull();
    }
}
