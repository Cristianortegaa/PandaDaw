using FluentValidation.TestHelper;
using PandaBack.Dtos.Ventas;
using PandaBack.Validators.Ventas;

namespace Tests.Validators
{
    public class CreateVentaValidatorTest
    {
        private CreateVentaValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new CreateVentaValidator();
        }

        [Test]
        public void DireccionEnvio_Vacia_DebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_MuyCorta_DebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = "AB" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_4Caracteres_DebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = "ABCD" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_5Caracteres_NoDebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = "ABCDE" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_300Caracteres_NoDebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = new string('A', 300) };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_301Caracteres_DebeHaberError()
        {
            var dto = new CreateVentaDto { DireccionEnvio = new string('A', 301) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.DireccionEnvio);
        }

        [Test]
        public void DireccionEnvio_Valida_NoDebeHaberErrores()
        {
            var dto = new CreateVentaDto { DireccionEnvio = "Calle Mayor 15, 3ºA, 28001 Madrid, España" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
