namespace PulsoUrbano.Net.Models.DTOs;

public record ZonaResumoDTO(int Id, string Nome, int Total);

public record EstatisticasResumoDTO(
    int TotalZonas,
    int TotalAlertas30dias,
    ZonaResumoDTO ZonaComMaisAlertas,
    string NivelPredominante,
    DateTime DtAtualizacao);
