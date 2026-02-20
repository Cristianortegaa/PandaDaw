using FluentValidation;
using PandaBack.Dtos.Favoritos;

namespace PandaBack.Validators.Favoritos;

/// <summary>
/// Validador FluentValidation para CreateFavoritoDto.
/// Reglas: ProductoId(>0).
/// </summary>
public class CreateFavoritoValidator : AbstractValidator<CreateFavoritoDto>
{
    /// <summary>
    /// Define reglas de validación para CreateFavoritoDto.
    /// </summary>
    public CreateFavoritoValidator()
    {
        RuleFor(f => f.ProductoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un producto válido");
    }
}

