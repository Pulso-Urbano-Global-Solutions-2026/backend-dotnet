using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using PulsoUrbano.Net.Middleware;

namespace PulsoUrbano.Net.Tests.Middleware;

public class JwtValidationMiddlewareTests
{
    private const string Secret = "test-secret-key-for-unit-tests-32chars!!";

    private static JwtValidationMiddleware Build(RequestDelegate next)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Secret"] = Secret })
            .Build();
        return new JwtValidationMiddleware(next, config, NullLogger<JwtValidationMiddleware>.Instance);
    }

    private static string MakeToken(int expiryMinutes = 60)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim("usuarioId", "42"), new Claim("email", "a@b.com"), new Claim("role", "USER")],
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static HttpContext MakeContext(string method, string path, string? authHeader = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.Path   = path;
        ctx.Response.Body  = new MemoryStream();
        if (authHeader is not null)
            ctx.Request.Headers.Authorization = authHeader;
        return ctx;
    }

    // ── N-17: core validation ─────────────────────────────────────────────

    [Fact]
    public async Task NoToken_ProtectedRoute_Returns401()
    {
        var ctx        = MakeContext("POST", "/api/alertas");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ValidToken_PassesThrough_SetsClaims()
    {
        var ctx        = MakeContext("POST", "/api/alertas", $"Bearer {MakeToken()}");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
        ctx.Items["JwtClaims"].Should().BeAssignableTo<ClaimsPrincipal>();
    }

    [Fact]
    public async Task ExpiredToken_Returns401()
    {
        var ctx        = MakeContext("POST", "/api/alertas", $"Bearer {MakeToken(-1)}");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(401);
    }

    // ── N-18: public-route bypass ─────────────────────────────────────────

    [Fact]
    public async Task GetAlertas_NoToken_PassesThrough()
    {
        var ctx        = MakeContext("GET", "/api/alertas");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PostAlertas_NoToken_Returns401()
    {
        var ctx        = MakeContext("POST", "/api/alertas");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Health_NoToken_PassesThrough()
    {
        var ctx        = MakeContext("GET", "/api/health");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Swagger_NoToken_PassesThrough()
    {
        var ctx        = MakeContext("GET", "/swagger/index.html");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ValidToken_InsufficientRole_Returns403()
    {
        // Arrange: token has role USER, but route requires ADMIN
        var ctx = MakeContext("POST", "/api/alertas", $"Bearer {MakeToken()}");
        ctx.Items["RequiredRole"] = "ADMIN"; // set by a hypothetical controller filter
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task ValidToken_SufficientRole_PassesThrough()
    {
        var ctx = MakeContext("POST", "/api/alertas", $"Bearer {MakeToken()}");
        ctx.Items["RequiredRole"] = "USER"; // USER matches the token's role claim
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAlerta_NoToken_Returns401()
    {
        var ctx        = MakeContext("DELETE", "/api/alertas/1");
        bool nextCalled = false;
        await Build(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(401);
    }
}
