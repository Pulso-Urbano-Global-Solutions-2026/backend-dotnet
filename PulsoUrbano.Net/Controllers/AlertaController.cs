using Microsoft.AspNetCore.Mvc;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Services;

namespace PulsoUrbano.Net.Controllers;

[ApiController]
[Route("api/alertas")]
[Produces("application/json")]
public class AlertaController(IAlertaService service) : ControllerBase
{
    /// <summary>Cria um novo alerta histórico.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AlertaResponseDTO),  StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO),   StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO),   StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO),   StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] AlertaCreateDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Lista alertas com filtro opcional por zona e janela de dias.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponseDTO<AlertaResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? zonaId,
        [FromQuery] int dias          = 30,
        [FromQuery] int pagina        = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var result = await service.GetAsync(zonaId, dias, pagina, tamanhoPagina);
        return Ok(result);
    }

    /// <summary>Retorna um alerta pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AlertaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Confirma ou desconfirma um alerta.</summary>
    [HttpPut("{id:int}/confirmar")]
    [ProducesResponseType(typeof(AlertaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(int id, [FromBody] AlertaConfirmarDTO dto)
    {
        var result = await service.ConfirmarAsync(id, dto.Confirmado);
        return Ok(result);
    }

    /// <summary>Remove um alerta permanentemente.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
