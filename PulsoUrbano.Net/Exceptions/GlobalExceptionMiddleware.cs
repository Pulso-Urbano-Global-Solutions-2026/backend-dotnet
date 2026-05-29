using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Resource not found");
            await Write(ctx, 404, new ErrorResponseDTO(404, "Não encontrado", ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed");
            var campos = ex.Errors.Select(e => e.ErrorMessage);
            await Write(ctx, 400, new ErrorResponseDTO(400, "Dados inválidos", Campos: campos));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database conflict");
            await Write(ctx, 409, new ErrorResponseDTO(409, "Conflito de dados"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized");
            await Write(ctx, 401, new ErrorResponseDTO(401, "Não autorizado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            // never expose stack trace in the response body
            await Write(ctx, 500, new ErrorResponseDTO(500, "Erro interno"));
        }
    }

    private static async Task Write(HttpContext ctx, int status, ErrorResponseDTO dto)
    {
        ctx.Response.StatusCode  = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(dto,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
