using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Services;

public interface IAlertaService
{
    Task<AlertaResponseDTO> CreateAsync(AlertaCreateDTO dto);
    Task<AlertaResponseDTO> GetByIdAsync(int id);
    Task<PaginatedResponseDTO<AlertaResponseDTO>> GetAsync(
        int? zonaId, int dias = 30, int pagina = 1, int tamanhoPagina = 20);
    Task<AlertaResponseDTO> ConfirmarAsync(int id, bool confirmado);
    Task DeleteAsync(int id);
}
