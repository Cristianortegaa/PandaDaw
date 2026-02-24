using FluentValidation.TestHelper;
using PandaBack.Dtos.Carrito;
using PandaBack.Validators.Carrito;

namespace Tests.Validators
{
    public class AddLineaCarritoRequestValidatorTest
    {
        private AddLineaCarritoRequestValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new AddLineaCarritoRequestValidator();
        }

        [Test]
        public void ProductoId_MayorQueCero_NoDebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 1, Cantidad = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_CeroONegativo_DebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 0, Cantidad = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Negativo_DebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = -5, Cantidad = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void Cantidad_MayorOIgualAUno_NoDebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 1, Cantidad = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void Cantidad_Cero_DebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 1, Cantidad = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void Cantidad_Negativa_DebeHaberError()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 1, Cantidad = -3 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void TodosLosCamposValidos_NoDebeHaberErrores()
        {
            var dto = new AddLineaCarritoRequestDto { ProductoId = 5, Cantidad = 10 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class UpdateLineaCarritoRequestValidatorTest
    {
        private UpdateLineaCarritoRequestValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new UpdateLineaCarritoRequestValidator();
        }

        [Test]
        public void Cantidad_MayorOIgualAUno_NoDebeHaberError()
        {
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 1 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void Cantidad_Cero_DebeHaberError()
        {
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void Cantidad_Negativa_DebeHaberError()
        {
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = -1 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Cantidad);
        }

        [Test]
        public void Cantidad_Alta_NoDebeHaberError()
        {
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 999 };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
