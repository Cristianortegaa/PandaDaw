using FluentValidation.TestHelper;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Validators.Valoraciones;

namespace Tests.Validators
{
    public class CreateValoracionValidatorTest
    {
        private CreateValoracionValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new CreateValoracionValidator();
        }

        // ==========================================
        // ProductoId
        // ==========================================

        [Test]
        public void ProductoId_MayorQueCero_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 3, Resena = "Bien" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Cero_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 0, Estrellas = 3, Resena = "Bien" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        [Test]
        public void ProductoId_Negativo_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = -1, Estrellas = 3, Resena = "Bien" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.ProductoId);
        }

        // ==========================================
        // Estrellas
        // ==========================================

        [Test]
        public void Estrellas_Uno_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 1, Resena = "Malo" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Estrellas);
        }

        [Test]
        public void Estrellas_Cinco_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 5, Resena = "Excelente" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Estrellas);
        }

        [Test]
        public void Estrellas_Tres_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 3, Resena = "Regular" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Estrellas);
        }

        [Test]
        public void Estrellas_Cero_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 0, Resena = "Test" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Estrellas);
        }

        [Test]
        public void Estrellas_Seis_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 6, Resena = "Test" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Estrellas);
        }

        [Test]
        public void Estrellas_Negativa_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = -1, Resena = "Test" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Estrellas);
        }

        // ==========================================
        // Reseña
        // ==========================================

        [Test]
        public void Resena_Vacia_DebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 3, Resena = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Resena);
        }

        [Test]
        public void Resena_MuyLarga_DebeHaberError()
        {
            var dto = new CreateValoracionDto
            {
                ProductoId = 1,
                Estrellas = 3,
                Resena = new string('A', 501)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Resena);
        }

        [Test]
        public void Resena_500Caracteres_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto
            {
                ProductoId = 1,
                Estrellas = 3,
                Resena = new string('A', 500)
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Resena);
        }

        [Test]
        public void Resena_Valida_NoDebeHaberError()
        {
            var dto = new CreateValoracionDto { ProductoId = 1, Estrellas = 4, Resena = "Muy buen producto" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Resena);
        }

        // ==========================================
        // Todo válido
        // ==========================================

        [Test]
        public void TodosLosCamposValidos_NoDebeHaberErrores()
        {
            var dto = new CreateValoracionDto
            {
                ProductoId = 5,
                Estrellas = 4,
                Resena = "Muy buen producto, lo recomiendo"
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
