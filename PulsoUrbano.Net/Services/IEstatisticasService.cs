using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Services;

public interface IEstatisticasService
{
    Task<EstatisticasZonaDTO> GetByZonaAsync(int zonaId, int dias = 30);
    Task<EstatisticasResumoDTO> GetResumoGeralAsync();
}
