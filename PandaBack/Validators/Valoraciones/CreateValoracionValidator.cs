using FluentValidation;
using PandaBack.Dtos.Valoraciones;

namespace PandaBack.Validators.Valoraciones;

/// <summary>
/// Validador FluentValidation para CreateValoracionDto.
/// Reglas: ProductoId(>0), Estrellas(1-5), Resena(obligatoria, max 500).
/// </summary>
public class CreateValoracionValidator : AbstractValidator<CreateValoracionDto>
{
    /// <summary>
    /// Define reglas de validación para CreateValoracionDto.
    /// </summary>
    public CreateValoracionValidator()
    {
        RuleFor(v => v.ProductoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un producto válido");

        RuleFor(v => v.Estrellas)
            .InclusiveBetween(1, 5).WithMessage("La valoración debe estar entre 1 y 5 estrellas");

        RuleFor(v => v.Resena)
            .NotEmpty().WithMessage("La reseña es obligatoria")
            .MaximumLength(500).WithMessage("La reseña no puede exceder los 500 caracteres");
    }
}

