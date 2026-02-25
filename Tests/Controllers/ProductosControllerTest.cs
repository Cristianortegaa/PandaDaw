using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Productos;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.RestController;
using PandaBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class ProductosControllerTest
    {
        private ProductosController _controller;
        private Mock<IProductoService> _serviceFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<IProductoService>();
            _controller = new ProductosController(_serviceFalso.Object);
        }

        // ==========================================
        // 1. PRUEBAS DE OBTENER TODOS LOS PRODUCTOS
        // ==========================================

        [Test]
        public async Task GetAll_SiHayProductos_DebeDevolver200ConLista()
        {
            // PREPARAR
            var listaProductos = new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Auriculares", Precio = 50, Stock = 10, Category = Categoria.Audio },
                new Producto { Id = 2, Nombre = "Monitor", Precio = 300, Stock = 5, Category = Categoria.Imagen }
            };

            _serviceFalso.Setup(s => s.GetAllProductosAsync())
                .ReturnsAsync(Result.Success<IEnumerable<Producto>, PandaError>(listaProductos));

            // ACTUAR
            var resultado = await _controller.GetAllAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAll_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetAllProductosAsync())
                .ReturnsAsync(Result.Failure<IEnumerable<Producto>, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetAllAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 2. PRUEBAS DE OBTENER PRODUCTO POR ID
        // ==========================================

        [Test]
        public async Task GetById_SiExiste_DebeDevolver200ConProducto()
        {
            // PREPARAR
            var producto = new Producto { Id = 1, Nombre = "Auriculares", Precio = 50, Stock = 10, Category = Categoria.Audio };

            _serviceFalso.Setup(s => s.GetProductoByIdAsync(1))
                .ReturnsAsync(Result.Success<Producto, PandaError>(producto));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetProductoByIdAsync(99))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetById_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetProductoByIdAsync(1))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new BadRequestError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
        }

        // ==========================================
        // 3. PRUEBAS DE OBTENER PRODUCTOS POR CATEGORÍA
        // ==========================================

        [Test]
        public async Task GetByCategory_SiCategoriaEsValida_DebeDevolver200()
        {
            // PREPARAR
            var listaProductos = new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Auriculares", Precio = 50, Stock = 10, Category = Categoria.Audio }
            };

            _serviceFalso.Setup(s => s.GetProductosByCategoryAsync(Categoria.Audio))
                .ReturnsAsync(Result.Success<IEnumerable<Producto>, PandaError>(listaProductos));

            // ACTUAR
            var resultado = await _controller.GetByCategoryAsync("Audio");

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetByCategory_SiCategoriaNoEsValida_DebeDevolver400()
        {
            // ACTUAR
            var resultado = await _controller.GetByCategoryAsync("CategoriaInventada");

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetByCategory_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetProductosByCategoryAsync(Categoria.Audio))
                .ReturnsAsync(Result.Failure<IEnumerable<Producto>, PandaError>(new BadRequestError("Error")));

            // ACTUAR
            var resultado = await _controller.GetByCategoryAsync("Audio");

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 4. PRUEBAS DE CREAR PRODUCTO
        // ==========================================

        [Test]
        public async Task Create_ConDatosCorrectos_DebeDevolver201()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares", Precio = 50, Stock = 10, Categoria = "Audio" };
            var productoCreado = new Producto { Id = 1, Nombre = "Auriculares", Precio = 50, Stock = 10, Category = Categoria.Audio };

            _serviceFalso.Setup(s => s.CreateProductoAsync(It.IsAny<Producto>()))
                .ReturnsAsync(Result.Success<Producto, PandaError>(productoCreado));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<CreatedResult>());
        }

        [Test]
        public async Task Create_ConDatosInvalidos_DebeDevolver400()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares", Precio = -10, Stock = 5, Categoria = "Audio" };

            _serviceFalso.Setup(s => s.CreateProductoAsync(It.IsAny<Producto>()))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new BadRequestError("Precio inválido")));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares", Precio = 50, Stock = 10, Categoria = "Audio" };

            _serviceFalso.Setup(s => s.CreateProductoAsync(It.IsAny<Producto>()))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new ConflictError("Error")));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 5. PRUEBAS DE ACTUALIZAR PRODUCTO
        // ==========================================

        [Test]
        public async Task Update_ConDatosCorrectos_DebeDevolver200()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares Pro", Precio = 80, Stock = 15, Categoria = "Audio" };
            var productoActualizado = new Producto { Id = 1, Nombre = "Auriculares Pro", Precio = 80, Stock = 15, Category = Categoria.Audio };

            _serviceFalso.Setup(s => s.UpdateProductoAsync(1, It.IsAny<Producto>()))
                .ReturnsAsync(Result.Success<Producto, PandaError>(productoActualizado));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Update_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares", Precio = 50, Stock = 10, Categoria = "Audio" };

            _serviceFalso.Setup(s => s.UpdateProductoAsync(99, It.IsAny<Producto>()))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(99, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Update_ConDatosInvalidos_DebeDevolver400()
        {
            // PREPARAR
            var dto = new ProductoRequestDto { Nombre = "Auriculares", Precio = -10, Stock = 5, Categoria = "Audio" };

            _serviceFalso.Setup(s => s.UpdateProductoAsync(1, It.IsAny<Producto>()))
                .ReturnsAsync(Result.Failure<Producto, PandaError>(new BadRequestError("Precio inválido")));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ==========================================
        // 6. PRUEBAS DE ELIMINAR PRODUCTO
        // ==========================================

        [Test]
        public async Task Delete_SiExiste_DebeDevolver204()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteProductoAsync(1))
                .ReturnsAsync(UnitResult.Success<PandaError>());

            // ACTUAR
            var resultado = await _controller.DeleteAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Delete_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteProductoAsync(99))
                .ReturnsAsync(UnitResult.Failure(new NotFoundError("Producto no encontrado") as PandaError));

            // ACTUAR
            var resultado = await _controller.DeleteAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Delete_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteProductoAsync(1))
                .ReturnsAsync(UnitResult.Failure(new ConflictError("Error") as PandaError));

            // ACTUAR
            var resultado = await _controller.DeleteAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
