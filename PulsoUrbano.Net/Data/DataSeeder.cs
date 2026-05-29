using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ZonasReferencia.AnyAsync()) return;

        var rng = new Random(562999);

        var zonas = new List<ZonaReferencia>
        {
            new() { Nome = "Centro",     Municipio = "São Paulo" },
            new() { Nome = "Zona Leste", Municipio = "São Paulo" },
            new() { Nome = "Zona Sul",   Municipio = "São Paulo" },
            new() { Nome = "Zona Norte", Municipio = "São Paulo" },
            new() { Nome = "Zona Oeste", Municipio = "São Paulo" }
        };

        await db.ZonasReferencia.AddRangeAsync(zonas);
        await db.SaveChangesAsync(); // HiLo IDs assigned here

        var alertas = new List<AlertaHistorico>();

        // Centro: 12 alertas, peso alto em EMERGENCIA → "zonaComMaisAlertas" para o demo
        alertas.AddRange(BuildAlertas(zonas[0], 12, rng, emergenciaWeight: 4));
        // Demais zonas: 8 alertas cada
        foreach (var zona in zonas.Skip(1))
            alertas.AddRange(BuildAlertas(zona, 8, rng, emergenciaWeight: 1));

        await db.AlertasHistorico.AddRangeAsync(alertas);
        await db.SaveChangesAsync();
    }

    private static IEnumerable<AlertaHistorico> BuildAlertas(
        ZonaReferencia zona, int count, Random rng, int emergenciaWeight)
    {
        var pool = Enumerable.Repeat("EMERGENCIA", emergenciaWeight)
            .Concat(Enumerable.Repeat("ALERTA", 2))
            .Concat(Enumerable.Repeat("ATENCAO", 2))
            .ToArray();

        for (int i = 0; i < count; i++)
        {
            var nivel = pool[rng.Next(pool.Length)];
            double score = nivel switch
            {
                "EMERGENCIA" => 20 + rng.NextDouble() * 20,
                "ALERTA"     => 40 + rng.NextDouble() * 20,
                _            => 60 + rng.NextDouble() * 20
            };
            double no2 = 20 + rng.NextDouble() * 35;

            yield return new AlertaHistorico
            {
                ZonaId            = zona.Id,
                NivelAlerta       = nivel,
                ScoreRegistrado   = Math.Round(score, 2),
                No2Registrado     = Math.Round(no2, 4),
                TextoRecomendacao = BuildTexto(nivel, zona.Nome, score),
                DtAlerta          = DateTime.UtcNow.AddDays(-rng.Next(0, 31)),
                Confirmado        = rng.Next(2) == 1
            };
        }
    }

    private static string BuildTexto(string nivel, string zona, double score) => nivel switch
    {
        "EMERGENCIA" => $"Qualidade do ar crítica em {zona}. Score {score:F1}. Permaneça em ambientes fechados.",
        "ALERTA"     => $"Qualidade do ar ruim em {zona}. Score {score:F1}. Evite esforço físico ao ar livre.",
        _            => $"Qualidade do ar moderada em {zona}. Score {score:F1}. Prefira sair antes das 10h."
    };
}
