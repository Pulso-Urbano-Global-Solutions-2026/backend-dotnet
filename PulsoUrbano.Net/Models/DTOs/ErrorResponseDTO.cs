namespace PulsoUrbano.Net.Models.DTOs;

public record ErrorResponseDTO(
    int Status,
    string Erro,
    string? Mensagem = null,
    IEnumerable<string>? Campos = null);
