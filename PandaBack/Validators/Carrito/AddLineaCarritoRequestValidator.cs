using FluentValidation;
using PandaBack.Dtos.Carrito;

namespace PandaBack.Validators.Carrito;

/// <summary>
/// Validador FluentValidation para AddLineaCarritoRequestDto.
/// Reglas: ProductoId(>0), Cantidad(>=1).
/// </summary>
public class AddLineaCarritoRequestValidator : AbstractValidator<AddLineaCarritoRequestDto>
{
    /// <summary>
    /// Define reglas de validación para AddLineaCarritoRequestDto.
    /// </summary>
    public AddLineaCarritoRequestValidator()
    {
        RuleFor(l => l.ProductoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un producto válido");

        RuleFor(l => l.Cantidad)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1");
    }
}

