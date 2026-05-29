using Microsoft.AspNetCore.Mvc;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Services;

namespace PulsoUrbano.Net.Controllers;

[ApiController]
[Route("api/estatisticas")]
public class EstatisticasController(IEstatisticasService service) : ControllerBase
{
    /// <summary>Retorna estatísticas agregadas de uma zona no período.</summary>
    [HttpGet("zona/{zonaId:int}")]
    [ProducesResponseType(typeof(EstatisticasZonaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByZona(int zonaId, [FromQuery] int dias = 30)
    {
        var result = await service.GetByZonaAsync(zonaId, dias);
        return Ok(result);
    }

    /// <summary>Retorna o resumo geral dos alertas dos últimos 30 dias.</summary>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(EstatisticasResumoDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumo()
    {
        var result = await service.GetResumoGeralAsync();
        return Ok(result);
    }
}
