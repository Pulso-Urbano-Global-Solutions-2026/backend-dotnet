namespace PulsoUrbano.Net.Models.DTOs;

public record PeriodoDTO(DateTime Inicio, DateTime Fim);

public record EstatisticasZonaDTO(
    int ZonaId,
    string ZonaNome,
    PeriodoDTO Periodo,
    int TotalAlertas,
    Dictionary<string, int> AlertasPorNivel,
    double ScoreMinimo,
    double ScoreMaximo,
    double ScoreMedia,
    int DiasComAlerta,
    DateTime? PiorDia,
    string Tendencia);  // MELHORANDO | PIORANDO | ESTAVEL
