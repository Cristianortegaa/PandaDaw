using FluentValidation;
using PandaBack.Dtos.Productos;

namespace PandaBack.Validators.Productos;

/// <summary>
/// Validador FluentValidation para ProductoRequestDto.
/// Reglas: Nombre(obligatorio), Precio(>=0), Stock(>=0), Categoria(obligatoria).
/// </summary>
public class ProductoRequestValidator : AbstractValidator<ProductoRequestDto>
{
    /// <summary>
    /// Define reglas de validación para ProductoRequestDto.
    /// </summary>
    public ProductoRequestValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(p => p.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

        RuleFor(p => p.Categoria)
            .NotEmpty().WithMessage("La categoría es obligatoria");
    }
}

