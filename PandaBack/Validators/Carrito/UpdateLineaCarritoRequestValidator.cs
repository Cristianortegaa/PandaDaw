using FluentValidation;
using PandaBack.Dtos.Carrito;

namespace PandaBack.Validators.Carrito;

/// <summary>
/// Validador FluentValidation para UpdateLineaCarritoRequestDto.
/// Reglas: Cantidad(>=1).
/// </summary>
public class UpdateLineaCarritoRequestValidator : AbstractValidator<UpdateLineaCarritoRequestDto>
{
    /// <summary>
    /// Define reglas de validación para UpdateLineaCarritoRequestDto.
    /// </summary>
    public UpdateLineaCarritoRequestValidator()
    {
        RuleFor(l => l.Cantidad)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1");
    }
}

