using FluentAssertions;
using PulsoUrbano.Net.Models.DTOs;
using PulsoUrbano.Net.Validators;

namespace PulsoUrbano.Net.Tests.DTOs;

public class AlertaCreateDTOValidatorTests
{
    private readonly AlertaCreateDTOValidator _validator = new();

    private static AlertaCreateDTO Valid() => new(
        ZonaId: 1,
        NivelAlerta: "ALERTA",
        ScoreRegistrado: 55.0,
        No2Registrado: 32.5,
        TextoRecomendacao: "Evite esforço físico ao ar livre.");

    [Fact]
    public void Validate_ValidPayload_Passes()
    {
        var result = _validator.Validate(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("emergencia")]   // case-sensitive
    [InlineData("")]
    [InlineData("CRITICO")]
    public void Validate_NivelAlertaInvalid_Fails(string nivel)
    {
        var dto = Valid() with { NivelAlerta = nivel };
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("ATENCAO")]
    [InlineData("ALERTA")]
    [InlineData("EMERGENCIA")]
    public void Validate_NivelAlertaValid_Passes(string nivel)
    {
        var dto = Valid() with { NivelAlerta = nivel };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ScoreOver100_Fails()
    {
        var dto = Valid() with { ScoreRegistrado = 100.1 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ScoreNegative_Fails()
    {
        var dto = Valid() with { ScoreRegistrado = -0.1 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_No2Negative_Fails()
    {
        var dto = Valid() with { No2Registrado = -1.0 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ZonaIdZero_Fails()
    {
        var dto = Valid() with { ZonaId = 0 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TextoEmpty_Fails()
    {
        var dto = Valid() with { TextoRecomendacao = "" };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TextoOver1000Chars_Fails()
    {
        var dto = Valid() with { TextoRecomendacao = new string('x', 1001) };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
