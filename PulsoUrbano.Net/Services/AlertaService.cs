using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Services;

public class AlertaService : IAlertaService
{
    private readonly AppDbContext _db;

    public AlertaService(AppDbContext db) => _db = db;

    public async Task<AlertaResponseDTO> CreateAsync(AlertaCreateDTO dto)
    {
        var zona = await _db.ZonasReferencia.FindAsync(dto.ZonaId)
                   ?? throw new KeyNotFoundException($"Zona {dto.ZonaId} não encontrada.");

        var alerta = new AlertaHistorico
        {
            ZonaId            = dto.ZonaId,
            NivelAlerta       = dto.NivelAlerta,
            ScoreRegistrado   = dto.ScoreRegistrado,
            No2Registrado     = dto.No2Registrado,
            TextoRecomendacao = dto.TextoRecomendacao,
            DtAlerta          = DateTime.UtcNow,
            Confirmado        = false
        };

        _db.AlertasHistorico.Add(alerta);
        await _db.SaveChangesAsync();

        alerta.Zona = zona; // attach for the mapper (already loaded above)
        return AlertaResponseDTO.FromEntity(alerta);
    }

    public async Task<AlertaResponseDTO> GetByIdAsync(int id)
    {
        var alerta = await _db.AlertasHistorico
                         .AsNoTracking()
                         .Include(a => a.Zona)
                         .FirstOrDefaultAsync(a => a.Id == id)
                     ?? throw new KeyNotFoundException($"Alerta {id} não encontrado.");

        return AlertaResponseDTO.FromEntity(alerta);
    }

    public async Task<PaginatedResponseDTO<AlertaResponseDTO>> GetAsync(
        int? zonaId, int dias = 30, int pagina = 1, int tamanhoPagina = 20)
    {
        var since = DateTime.UtcNow.AddDays(-dias);

        var query = _db.AlertasHistorico
            .AsNoTracking()
            .Include(a => a.Zona)
            .Where(a => a.DtAlerta >= since);

        if (zonaId.HasValue)
            query = query.Where(a => a.ZonaId == zonaId.Value);

        var total = await query.CountAsync();

        var dados = (await query
            .OrderByDescending(a => a.DtAlerta)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync())
            .Select(AlertaResponseDTO.FromEntity)
            .ToList();

        return new PaginatedResponseDTO<AlertaResponseDTO>(total, pagina, tamanhoPagina, dados);
    }

    public async Task<AlertaResponseDTO> ConfirmarAsync(int id, bool confirmado)
    {
        var alerta = await _db.AlertasHistorico
                         .Include(a => a.Zona)
                         .FirstOrDefaultAsync(a => a.Id == id)
                     ?? throw new KeyNotFoundException($"Alerta {id} não encontrado.");

        alerta.Confirmado = confirmado;
        await _db.SaveChangesAsync();

        return AlertaResponseDTO.FromEntity(alerta);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete — no Ativo flag in entity; contract returns 204
        var alerta = await _db.AlertasHistorico.FindAsync(id)
                     ?? throw new KeyNotFoundException($"Alerta {id} não encontrado.");

        _db.AlertasHistorico.Remove(alerta);
        await _db.SaveChangesAsync();
    }
}
