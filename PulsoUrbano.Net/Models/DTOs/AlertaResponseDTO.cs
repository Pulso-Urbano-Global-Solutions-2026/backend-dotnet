using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Models.DTOs;

public record AlertaResponseDTO(
    int Id,
    int ZonaId,
    string ZonaNome,
    string NivelAlerta,
    double ScoreRegistrado,
    double No2Registrado,
    string TextoRecomendacao,
    DateTime DtAlerta,
    bool Confirmado)
{
    public static AlertaResponseDTO FromEntity(AlertaHistorico a) => new(
        a.Id,
        a.ZonaId,
        a.Zona?.Nome ?? string.Empty,
        a.NivelAlerta,
        a.ScoreRegistrado,
        a.No2Registrado,
        a.TextoRecomendacao,
        a.DtAlerta,
        a.Confirmado);
}
