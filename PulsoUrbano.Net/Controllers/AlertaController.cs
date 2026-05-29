using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Services;

namespace PulsoUrbano.Net.Controllers;

/// <summary>CRUD completo de alertas históricos de qualidade do ar.</summary>
[ApiController]
[Route("api/alertas")]
[Produces("application/json")]
public class AlertaController(IAlertaService service) : ControllerBase
{
    /// <summary>Cria um novo alerta histórico.</summary>
    /// <param name="dto">Dados do alerta: zonaId, nivelAlerta (ATENCAO|ALERTA|EMERGENCIA), scoreRegistrado, no2Registrado, textoRecomendacao.</param>
    /// <returns>O alerta criado com Id gerado e ZonaNome resolvido.</returns>
    /// <remarks>
    /// Exemplo de body:
    /// <code>{ "zonaId": 1, "nivelAlerta": "ALERTA", "scoreRegistrado": 38.5, "no2Registrado": 41.2, "textoRecomendacao": "Evite esforço físico ao ar livre." }</code>
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria alerta histórico", Description = "Persiste um alerta de qualidade do ar. Requer Bearer token.")]
    [SwaggerResponse(201, "Alerta criado com sucesso",    typeof(AlertaResponseDTO))]
    [SwaggerResponse(400, "Dados inválidos",              typeof(ErrorResponseDTO))]
    [SwaggerResponse(401, "Token ausente ou inválido",    typeof(ErrorResponseDTO))]
    [SwaggerResponse(404, "Zona não encontrada",          typeof(ErrorResponseDTO))]
    [ProducesResponseType(typeof(AlertaResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] AlertaCreateDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Lista alertas com paginação e filtros opcionais.</summary>
    /// <param name="zonaId">Filtra por zona (opcional).</param>
    /// <param name="dias">Janela de dias retroativos (padrão: 30).</param>
    /// <param name="pagina">Número da página (padrão: 1).</param>
    /// <param name="tamanhoPagina">Itens por página (padrão: 20).</param>
    /// <returns>Lista paginada de alertas.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista alertas", Description = "Rota pública. Filtre por zona e/ou janela de dias.")]
    [SwaggerResponse(200, "Lista paginada de alertas", typeof(PaginatedResponseDTO<AlertaResponseDTO>))]
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
    /// <param name="id">Identificador do alerta.</param>
    /// <returns>Alerta com ZonaNome resolvido.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Busca alerta por ID", Description = "Rota pública.")]
    [SwaggerResponse(200, "Alerta encontrado",   typeof(AlertaResponseDTO))]
    [SwaggerResponse(404, "Alerta não encontrado", typeof(ErrorResponseDTO))]
    [ProducesResponseType(typeof(AlertaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Confirma ou desconfirma um alerta.</summary>
    /// <param name="id">Identificador do alerta.</param>
    /// <param name="dto">Body: <c>{ "confirmado": true }</c></param>
    /// <returns>Alerta com campo Confirmado atualizado.</returns>
    [HttpPut("{id:int}/confirmar")]
    [SwaggerOperation(Summary = "Confirma alerta", Description = "Marca o alerta como confirmado ou desconfirmado. Requer Bearer token.")]
    [SwaggerResponse(200, "Alerta atualizado",         typeof(AlertaResponseDTO))]
    [SwaggerResponse(401, "Token ausente ou inválido", typeof(ErrorResponseDTO))]
    [SwaggerResponse(404, "Alerta não encontrado",     typeof(ErrorResponseDTO))]
    [ProducesResponseType(typeof(AlertaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO),  StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirmar(int id, [FromBody] AlertaConfirmarDTO dto)
    {
        var result = await service.ConfirmarAsync(id, dto.Confirmado);
        return Ok(result);
    }

    /// <summary>Remove um alerta permanentemente.</summary>
    /// <param name="id">Identificador do alerta.</param>
    /// <returns>204 No Content em caso de sucesso.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Remove alerta", Description = "Hard delete — sem soft delete. Requer Bearer token.")]
    [SwaggerResponse(204, "Alerta removido")]
    [SwaggerResponse(401, "Token ausente ou inválido", typeof(ErrorResponseDTO))]
    [SwaggerResponse(404, "Alerta não encontrado",     typeof(ErrorResponseDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
