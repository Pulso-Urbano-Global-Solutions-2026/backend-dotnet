using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Services;

public class EstatisticasService : IEstatisticasService
{
    private readonly AppDbContext _db;

    public EstatisticasService(AppDbContext db) => _db = db;

    public async Task<EstatisticasZonaDTO> GetByZonaAsync(int zonaId, int dias = 30)
    {
        var zona = await _db.ZonasReferencia
                       .AsNoTracking()
                       .FirstOrDefaultAsync(z => z.Id == zonaId)
                   ?? throw new KeyNotFoundException($"Zona {zonaId} não encontrada.");

        var since = DateTime.UtcNow.AddDays(-dias);

        // Materialize minimal columns; aggregate in memory (documented: avoids Oracle provider
        // limitations with complex grouping expressions on the index IX_ALERTA_ZONA_DT)
        var alertas = await _db.AlertasHistorico
            .AsNoTracking()
            .Where(a => a.ZonaId == zonaId && a.DtAlerta >= since)
            .Select(a => new { a.NivelAlerta, a.ScoreRegistrado, a.DtAlerta })
            .ToListAsync();

        if (!alertas.Any())
            return new EstatisticasZonaDTO(
                zonaId, zona.Nome, new PeriodoDTO(since, DateTime.UtcNow),
                0, [], 0, 0, 0, 0, null, "ESTAVEL");

        var alertasPorNivel = alertas
            .GroupBy(a => a.NivelAlerta)
            .ToDictionary(g => g.Key, g => g.Count());

        var scores     = alertas.Select(a => a.ScoreRegistrado).ToList();
        var scoreMin   = scores.Min();
        var scoreMax   = scores.Max();
        var scoreMedia = Math.Round(scores.Average(), 2);

        var diasComAlerta = alertas.Select(a => a.DtAlerta.Date).Distinct().Count();
        var piorDia       = alertas.MinBy(a => a.ScoreRegistrado)!.DtAlerta;

        // Tendencia: compare avg score of first half vs second half of the window
        // Threshold ±2 pts (documented assumption — stable for seeded demo data)
        var midpoint   = since.AddDays(dias / 2.0);
        var avgFirst   = alertas.Where(a => a.DtAlerta <  midpoint).Select(a => a.ScoreRegistrado).DefaultIfEmpty(scoreMedia).Average();
        var avgSecond  = alertas.Where(a => a.DtAlerta >= midpoint).Select(a => a.ScoreRegistrado).DefaultIfEmpty(scoreMedia).Average();

        string tendencia = (avgSecond - avgFirst) switch
        {
            > 2  => "MELHORANDO",
            < -2 => "PIORANDO",
            _    => "ESTAVEL"
        };

        return new EstatisticasZonaDTO(
            zonaId, zona.Nome, new PeriodoDTO(since, DateTime.UtcNow),
            alertas.Count, alertasPorNivel,
            scoreMin, scoreMax, scoreMedia,
            diasComAlerta, piorDia, tendencia);
    }

    public async Task<EstatisticasResumoDTO> GetResumoGeralAsync()
    {
        var since      = DateTime.UtcNow.AddDays(-30);
        var totalZonas = await _db.ZonasReferencia.AsNoTracking().CountAsync();

        var alertas30 = await _db.AlertasHistorico
            .AsNoTracking()
            .Where(a => a.DtAlerta >= since)
            .Select(a => new { a.ZonaId, a.NivelAlerta })
            .ToListAsync();

        var totalAlertas30 = alertas30.Count;

        // ZonaComMaisAlertas
        var topGroup = alertas30
            .GroupBy(a => a.ZonaId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        ZonaResumoDTO zonaComMaisAlertas;
        if (topGroup is not null)
        {
            var nomeZona = await _db.ZonasReferencia
                .AsNoTracking()
                .Where(z => z.Id == topGroup.Key)
                .Select(z => z.Nome)
                .FirstOrDefaultAsync() ?? string.Empty;

            zonaComMaisAlertas = new ZonaResumoDTO(topGroup.Key, nomeZona, topGroup.Count());
        }
        else
        {
            zonaComMaisAlertas = new ZonaResumoDTO(0, "N/A", 0);
        }

        // NivelPredominante (mode of NivelAlerta in 30 days)
        var nivelPredominante = alertas30
            .GroupBy(a => a.NivelAlerta)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "N/A";

        return new EstatisticasResumoDTO(
            totalZonas, totalAlertas30,
            zonaComMaisAlertas, nivelPredominante,
            DateTime.UtcNow);
    }
}
