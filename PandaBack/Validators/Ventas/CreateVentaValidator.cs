using FluentValidation;
using PandaBack.Dtos.Ventas;

namespace PandaBack.Validators.Ventas;

/// <summary>
/// Validador FluentValidation para CreateVentaDto.
/// Reglas: DireccionEnvio(obligatoria, 5-300 caracteres).
/// </summary>
public class CreateVentaValidator : AbstractValidator<CreateVentaDto>
{
    /// <summary>
    /// Define reglas de validación para CreateVentaDto.
    /// </summary>
    public CreateVentaValidator()
    {
        RuleFor(v => v.DireccionEnvio)
            .NotEmpty().WithMessage("La dirección de envío es obligatoria")
            .MinimumLength(5).WithMessage("La dirección de envío debe tener al menos 5 caracteres")
            .MaximumLength(300).WithMessage("La dirección de envío no puede exceder 300 caracteres");
    }
}

