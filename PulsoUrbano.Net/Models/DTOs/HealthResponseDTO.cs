namespace PulsoUrbano.Net.Models.DTOs;

public record HealthResponseDTO(
    string Status,
    string Servico,
    string Versao,
    DateTime Timestamp,
    string Database);
