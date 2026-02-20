using Moq;
using PandaBack.Services;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;

namespace Tests.Services
{
    public class ValoracionServiceTest
    {
        private ValoracionService _service;
        private Mock<IValoracionRepository> _repoValoracionesFalso;
        private Mock<IProductoRepository> _repoProductosFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _repoValoracionesFalso = new Mock<IValoracionRepository>();
            _repoProductosFalso = new Mock<IProductoRepository>();

            _service = new ValoracionService(_repoValoracionesFalso.Object, _repoProductosFalso.Object);
        }

        // ==========================================
        // 1. PRUEBAS DE OBTENER VALORACIONES POR PRODUCTO
        // ==========================================

        [Test]
        public async Task ObtenerPorProducto_SiProductoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoProductosFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.GetValoracionesByProductoAsync(99);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task ObtenerPorProducto_SiProductoExiste_DebeDevolverListaConExito()
        {
            // PREPARAR
            long productoId = 5;
            var producto = new Producto { Id = productoId, Nombre = "Auriculares" };
            var listaFalsa = new List<Valoracion>
            {
                new Valoracion { Id = 1, ProductoId = productoId, Estrellas = 5, Resena = "Genial", Producto = producto }
            };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(productoId)).ReturnsAsync(producto);
            _repoValoracionesFalso.Setup(r => r.GetByProductoIdAsync(productoId)).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetValoracionesByProductoAsync(productoId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(1));
        }

        // ==========================================
        // 2. PRUEBAS DE OBTENER VALORACIONES POR USUARIO
        // ==========================================

        [Test]
        public async Task ObtenerPorUsuario_DebeDevolverListaConExito()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares" };
            var listaFalsa = new List<Valoracion>
            {
                new Valoracion { Id = 1, UserId = userId, Estrellas = 4, Resena = "Bueno", Producto = producto }
            };

            _repoValoracionesFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetValoracionesByUserAsync(userId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(1));
        }

        // ==========================================
        // 3. PRUEBAS DE CREAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task CrearValoracion_SiProductoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 99, Estrellas = 5, Resena = "Genial" };
            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.CreateValoracionAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task CrearValoracion_SiYaHaValorado_DebeDarErrorConflict()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 5, Resena = "Genial" };
            var producto = new Producto { Id = 5 };
            var valoracionExistente = new Valoracion { ProductoId = 5, UserId = 1 };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync(producto);
            _repoValoracionesFalso.Setup(r => r.GetByProductoAndUserAsync(dto.ProductoId, 1)).ReturnsAsync(valoracionExistente);

            // ACTUAR
            var resultado = await _service.CreateValoracionAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<ConflictError>());
        }

        [Test]
        public async Task CrearValoracion_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 4, Resena = "Muy buen producto" };
            var producto = new Producto { Id = 5, Nombre = "Auriculares" };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync(producto);
            _repoValoracionesFalso.Setup(r => r.GetByProductoAndUserAsync(dto.ProductoId, 1)).ReturnsAsync((Valoracion)null);

            // ACTUAR
            var resultado = await _service.CreateValoracionAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Estrellas, Is.EqualTo(4));
            Assert.That(resultado.Value.Resena, Is.EqualTo("Muy buen producto"));
        }

        // ==========================================
        // 4. PRUEBAS DE ACTUALIZAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task ActualizarValoracion_SiNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "Regular" };
            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Valoracion)null);

            // ACTUAR
            var resultado = await _service.UpdateValoracionAsync(99, 1, dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task ActualizarValoracion_SiNoEsDelUsuario_DebeDarErrorOperacionNoPermitida()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "Regular" };
            var valoracionDeOtro = new Valoracion { Id = 10, UserId = 999, ProductoId = 5 };

            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(valoracionDeOtro);

            // ACTUAR
            var resultado = await _service.UpdateValoracionAsync(10, 1, dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<OperacionNoPermitidaError>());
        }

        [Test]
        public async Task ActualizarValoracion_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "Actualizado" };
            var producto = new Producto { Id = 5, Nombre = "Auriculares" };
            var valoracionExistente = new Valoracion
            {
                Id = 10, UserId = 1, ProductoId = 5, Estrellas = 5, Resena = "Vieja", Producto = producto
            };

            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(valoracionExistente);

            // ACTUAR
            var resultado = await _service.UpdateValoracionAsync(10, 1, dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Estrellas, Is.EqualTo(3));
            Assert.That(resultado.Value.Resena, Is.EqualTo("Actualizado"));
        }

        // ==========================================
        // 5. PRUEBAS DE ELIMINAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task EliminarValoracion_SiNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Valoracion)null);

            // ACTUAR
            var resultado = await _service.DeleteValoracionAsync(99, 1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task EliminarValoracion_SiNoEsDelUsuario_DebeDarErrorOperacionNoPermitida()
        {
            // PREPARAR
            var valoracionDeOtro = new Valoracion { Id = 10, UserId = 999 };
            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(valoracionDeOtro);

            // ACTUAR
            var resultado = await _service.DeleteValoracionAsync(10, 1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<OperacionNoPermitidaError>());
        }

        [Test]
        public async Task EliminarValoracion_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var valoracionReal = new Valoracion { Id = 10, UserId = 1 };
            _repoValoracionesFalso.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(valoracionReal);

            // ACTUAR
            var resultado = await _service.DeleteValoracionAsync(10, 1);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            _repoValoracionesFalso.Verify(r => r.DeleteAsync(10), Times.Once);
        }
    }
}

