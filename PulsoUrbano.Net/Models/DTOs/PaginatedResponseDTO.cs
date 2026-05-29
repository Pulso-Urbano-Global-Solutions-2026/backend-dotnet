namespace PulsoUrbano.Net.Models.DTOs;

public record PaginatedResponseDTO<T>(
    int Total,
    int Pagina,
    int TamanhoPagina,
    IReadOnlyList<T> Dados);
