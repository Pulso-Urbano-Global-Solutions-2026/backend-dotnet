using Microsoft.AspNetCore.Mvc;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(AppDbContext db) : ControllerBase
{
    /// <summary>Verifica a saúde da API e a conectividade com o banco.</summary>
    [HttpGet]
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
