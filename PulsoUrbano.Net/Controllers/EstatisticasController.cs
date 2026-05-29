using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Services;

namespace PulsoUrbano.Net.Controllers;

/// <summary>Estatísticas agregadas de alertas por zona e resumo geral.</summary>
[ApiController]
[Route("api/estatisticas")]
[Produces("application/json")]
public class EstatisticasController(IEstatisticasService service) : ControllerBase
{
    /// <summary>Retorna estatísticas agregadas de uma zona no período.</summary>
    /// <param name="zonaId">Identificador da zona (ex: 1 = Centro).</param>
    /// <param name="dias">Janela de dias retroativos (padrão: 30).</param>
    /// <returns>Estatísticas com score médio, dias críticos e tendência (MELHORANDO|PIORANDO|ESTAVEL).</returns>
    /// <remarks>
    /// Exemplo: <c>GET /api/estatisticas/zona/1?dias=30</c>
    /// A tendência compara a média de score da primeira metade vs segunda metade da janela (±2 pts de threshold).
    /// </remarks>
    [HttpGet("zona/{zonaId:int}")]
    [SwaggerOperation(Summary = "Estatísticas por zona", Description = "Rota pública. Retorna agregados e tendência de qualidade do ar para a zona.")]
    [SwaggerResponse(200, "Estatísticas calculadas",  typeof(EstatisticasZonaDTO))]
    [SwaggerResponse(404, "Zona não encontrada",      typeof(ErrorResponseDTO))]
    [ProducesResponseType(typeof(EstatisticasZonaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO),    StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByZona(int zonaId, [FromQuery] int dias = 30)
    {
        var result = await service.GetByZonaAsync(zonaId, dias);
        return Ok(result);
    }

    /// <summary>Retorna o resumo geral dos alertas dos últimos 30 dias.</summary>
    /// <returns>Totais globais, zona com mais alertas e nível predominante.</returns>
    /// <remarks>
    /// Exemplo: <c>GET /api/estatisticas/resumo</c>
    /// Usado no pitch: "qual zona teve mais emergências e a tendência está melhorando?"
    /// </remarks>
    [HttpGet("resumo")]
    [SwaggerOperation(Summary = "Resumo geral", Description = "Rota pública. Agrega todos os alertas dos últimos 30 dias e aponta a zona mais crítica.")]
    [SwaggerResponse(200, "Resumo calculado", typeof(EstatisticasResumoDTO))]
    [ProducesResponseType(typeof(EstatisticasResumoDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumo()
    {
        var result = await service.GetResumoGeralAsync();
        return Ok(result);
    }
}
