using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Controllers;

/// <summary>Endpoint de healthcheck para Docker e monitoramento.</summary>
[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController(AppDbContext db) : ControllerBase
{
    /// <summary>Verifica a saúde da API e a conectividade com o banco.</summary>
    /// <returns>Status da API e do banco de dados Oracle.</returns>
    /// <remarks>
    /// Usado pelo Docker Compose healthcheck: <c>curl -f http://localhost:5000/api/health</c>
    /// Resposta esperada: <c>{ "status": "healthy", "database": "connected" }</c>
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "Healthcheck", Description = "Rota pública. Verifica conectividade com Oracle via CanConnectAsync (sem query pesada).")]
    [SwaggerResponse(200, "API saudável", typeof(HealthResponseDTO))]
    [ProducesResponseType(typeof(HealthResponseDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        string database;
        try
        {
            database = await db.Database.CanConnectAsync() ? "connected" : "unreachable";
        }
        catch (Exception ex)
        {
            database = $"error: {ex.Message}";
        }

        return Ok(new HealthResponseDTO(
            Status:    "healthy",
            Servico:   "pulso-urbano-dotnet",
            Versao:    "1.0.0",
            Timestamp: DateTime.UtcNow,
            Database:  database));
    }
}
