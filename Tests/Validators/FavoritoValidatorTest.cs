using FluentValidation.TestHelper;
using PandaBack.Dtos.Favoritos;
using PandaBack.Validators.Favoritos;

namespace Tests.Validators
{
    public class CreateFavoritoValidatorTest
    {
        private CreateFavoritoValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new CreateFavoritoValidator();
        }

        [Test]
        public void ProductoId_MayorQueCero_NoDebeHaberError()
        {
            var dto = new CreateFavoritoDto { ProductoId = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Cero_DebeHaberError()
        {
            var dto = new CreateFavoritoDto { ProductoId = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Negativo_DebeHaberError()
        {
            var dto = new CreateFavoritoDto { ProductoId = -10 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Valido_NoDebeHaberErrores()
        {
            var dto = new CreateFavoritoDto { ProductoId = 42 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
