using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PulsoUrbano.Net.Exceptions;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Tests.Exceptions;

public class GlobalExceptionMiddlewareTests
{
    private static GlobalExceptionMiddleware Build(RequestDelegate next) =>
        new(next, NullLogger<GlobalExceptionMiddleware>.Instance);

    private static async Task<(int status, ErrorResponseDTO? body)> Run(Exception ex)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await Build(_ => throw ex).InvokeAsync(ctx);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        var dto  = JsonSerializer.Deserialize<ErrorResponseDTO>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return (ctx.Response.StatusCode, dto);
    }

    [Fact]
    public async Task KeyNotFound_Maps404()
    {
        var (status, body) = await Run(new KeyNotFoundException("alerta 99 não encontrado"));

        status.Should().Be(404);
        body!.Status.Should().Be(404);
        body.Erro.Should().Be("Não encontrado");
        body.Mensagem.Should().Contain("99");
    }

    [Fact]
    public async Task ValidationException_Maps400WithCampos()
    {
        var failures = new List<ValidationFailure>
        {
            new("NivelAlerta", "NivelAlerta deve ser ATENCAO, ALERTA ou EMERGENCIA."),
            new("ZonaId", "ZonaId deve ser maior que 0.")
        };
        var (status, body) = await Run(new ValidationException(failures));

        status.Should().Be(400);
        body!.Erro.Should().Be("Dados inválidos");
        body.Campos.Should().HaveCount(2);
    }

    [Fact]
    public async Task DbUpdateException_Maps409()
    {
        var (status, body) = await Run(new DbUpdateException("FK restrict violation"));

        status.Should().Be(409);
        body!.Erro.Should().Be("Conflito de dados");
    }

    [Fact]
    public async Task UnauthorizedAccess_Maps401()
    {
        var (status, body) = await Run(new UnauthorizedAccessException());

        status.Should().Be(401);
        body!.Erro.Should().Be("Não autorizado");
    }

    [Fact]
    public async Task Generic_Maps500_NoStackTraceInBody()
    {
        var (status, body) = await Run(new InvalidOperationException("boom"));

        status.Should().Be(500);
        body!.Erro.Should().Be("Erro interno");
        // stack trace must not leak
        body.Mensagem.Should().BeNull();
        JsonSerializer.Serialize(body).Should().NotContain("at ");
    }
}
