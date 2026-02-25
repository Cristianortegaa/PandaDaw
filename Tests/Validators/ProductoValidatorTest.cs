using FluentValidation.TestHelper;
using PandaBack.Dtos.Productos;
using PandaBack.Validators.Productos;

namespace Tests.Validators
{
    public class ProductoRequestValidatorTest
    {
        private ProductoRequestValidator _validator;

        [SetUp]
        public void PrepararTodo()
        {
            _validator = new ProductoRequestValidator();
        }

        // ==========================================
        // Nombre
        // ==========================================

        [Test]
        public void Nombre_Vacio_DebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "", Precio = 10, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Nombre);
        }

        [Test]
        public void Nombre_Null_DebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = null!, Precio = 10, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Nombre);
        }

        [Test]
        public void Nombre_MuyLargo_DebeHaberError()
        {
            var dto = new ProductoRequestDto
            {
                Nombre = new string('A', 201),
                Precio = 10,
                Stock = 5,
                Categoria = "Audio"
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Nombre);
        }

        [Test]
        public void Nombre_Valido_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Auriculares Sony", Precio = 10, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Nombre);
        }

        [Test]
        public void Nombre_200Caracteres_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto
            {
                Nombre = new string('A', 200),
                Precio = 10,
                Stock = 5,
                Categoria = "Audio"
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Nombre);
        }

        // ==========================================
        // Precio
        // ==========================================

        [Test]
        public void Precio_Negativo_DebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = -1, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Precio);
        }

        [Test]
        public void Precio_Cero_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 0, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Precio);
        }

        [Test]
        public void Precio_Positivo_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 99.99m, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Precio);
        }

        // ==========================================
        // Stock
        // ==========================================

        [Test]
        public void Stock_Negativo_DebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 10, Stock = -1, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Stock);
        }

        [Test]
        public void Stock_Cero_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 10, Stock = 0, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Stock);
        }

        [Test]
        public void Stock_Positivo_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 10, Stock = 100, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Stock);
        }

        // ==========================================
        // Categoría
        // ==========================================

        [Test]
        public void Categoria_Vacia_DebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 10, Stock = 5, Categoria = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Categoria);
        }

        [Test]
        public void Categoria_Valida_NoDebeHaberError()
        {
            var dto = new ProductoRequestDto { Nombre = "Test", Precio = 10, Stock = 5, Categoria = "Audio" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Categoria);
        }

        // ==========================================
        // Todo válido
        // ==========================================

        [Test]
        public void TodosLosCamposValidos_NoDebeHaberErrores()
        {
            var dto = new ProductoRequestDto
            {
                Nombre = "Auriculares Sony WH-1000XM5",
                Precio = 349.00m,
                Stock = 25,
                Categoria = "Audio"
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
