using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Middleware;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    // Public routes: no token required (reads + swagger + health)
    private static bool IsPublicRoute(HttpContext ctx)
    {
        var method = ctx.Request.Method;
        var path   = ctx.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/swagger",  StringComparison.OrdinalIgnoreCase)) return true;
        if (path.StartsWith("/api-docs", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/health",  StringComparison.OrdinalIgnoreCase))  return true;

        // All GETs on alerta + estatisticas are public (mobile reads without login)
        if (method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
            (path.StartsWith("/api/alertas",      StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/api/estatisticas", StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }

    public JwtValidationMiddleware(RequestDelegate next, IConfiguration config,
                                   ILogger<JwtValidationMiddleware> logger)
    {
        _next   = next;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (IsPublicRoute(ctx))
        {
            await _next(ctx);
            return;
        }

        var header = ctx.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await Reject(ctx, 401, "Token ausente ou inválido");
            return;
        }

        var token  = header["Bearer ".Length..];
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
                     ?? _config["Jwt:Secret"]
                     ?? "pulso-secret-2026";

        try
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear(); // preserve short claim names ("role", "email", etc.)
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = key,
                ValidateLifetime         = true,
                ValidateIssuer           = false, // Q-01: Java issuer varies by env
                ValidateAudience         = false, // Q-01: mobile clients skip audience
                ClockSkew                = TimeSpan.Zero
            }, out _);

            ctx.Items["JwtClaims"] = principal;

            // Role 403 hook: controllers set ctx.Items["RequiredRole"] to enforce access
            // No endpoint requires a specific role yet — hook is ready for future use
            if (ctx.Items.TryGetValue("RequiredRole", out var requiredRoleObj) &&
                requiredRoleObj is string requiredRole)
            {
                var userRole = principal.FindFirst("role")?.Value ?? string.Empty;
                if (!userRole.Contains(requiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    await Reject(ctx, 403, "Acesso negado: papel insuficiente");
                    return;
                }
            }

            await _next(ctx);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("JWT rejected: expired");
            await Reject(ctx, 401, "Token expirado");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT rejected: invalid");
            await Reject(ctx, 401, "Token ausente ou inválido");
        }
    }

    private static async Task Reject(HttpContext ctx, int status, string erro)
    {
        ctx.Response.StatusCode  = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new ErrorResponseDTO(status, erro));
    }
}
