using FluentValidation;
using PulsoUrbano.Net.Models.DTOs;

namespace PulsoUrbano.Net.Validators;

public class AlertaCreateDTOValidator : AbstractValidator<AlertaCreateDTO>
{
    private static readonly string[] NiveisPermitidos = ["ATENCAO", "ALERTA", "EMERGENCIA"];

    public AlertaCreateDTOValidator()
    {
        RuleFor(x => x.ZonaId)
            .GreaterThan(0);

        RuleFor(x => x.NivelAlerta)
            .NotEmpty()
            .Must(n => NiveisPermitidos.Contains(n))
            .WithMessage("NivelAlerta deve ser ATENCAO, ALERTA ou EMERGENCIA.");

        RuleFor(x => x.ScoreRegistrado)
            .InclusiveBetween(0.0, 100.0);

        RuleFor(x => x.No2Registrado)
            .GreaterThanOrEqualTo(0.0);

        RuleFor(x => x.TextoRecomendacao)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
