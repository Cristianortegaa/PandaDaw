using PandaBack.Models;

namespace Tests.Models
{
    public class VentaModelTest
    {
        [Test]
        public void AddLineaVenta_DebeAgregarLineaYCalcularTotal()
        {
            var venta = new Venta { UserId = "user-1" };
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 100 };
            var linea = new LineaVenta
            {
                ProductoId = 1,
                Cantidad = 2,
                PrecioUnitario = 100,
                Producto = producto
            };

            venta.AddLineaVenta(linea);

            Assert.That(venta.Lineas.Count, Is.EqualTo(1));
            Assert.That(venta.Total, Is.EqualTo(200));
        }

        [Test]
        public void AddLineaVenta_MultiplesLineas_DebeCalcularTotalCorrectamente()
        {
            var venta = new Venta { UserId = "user-1" };

            venta.AddLineaVenta(new LineaVenta { ProductoId = 1, Cantidad = 2, PrecioUnitario = 100 });
            venta.AddLineaVenta(new LineaVenta { ProductoId = 2, Cantidad = 1, PrecioUnitario = 50 });

            Assert.That(venta.Lineas.Count, Is.EqualTo(2));
            Assert.That(venta.Total, Is.EqualTo(250));
        }

        [Test]
        public void CalculateTotal_DebeRecalcularCorrectamente()
        {
            var venta = new Venta { UserId = "user-1" };
            venta.Lineas.Add(new LineaVenta { Cantidad = 3, PrecioUnitario = 10 });
            venta.Lineas.Add(new LineaVenta { Cantidad = 1, PrecioUnitario = 25 });

            venta.CalculateTotal();

            Assert.That(venta.Total, Is.EqualTo(55));
        }

        // ==========================================
        // UpdateEstado - Transiciones válidas
        // ==========================================

        [Test]
        public void UpdateEstado_DePendienteAProcesando_DebeTenerExito()
        {
            var venta = new Venta { Estado = EstadoPedido.Pendiente };

            venta.UpdateEstado(EstadoPedido.Procesando);

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Procesando));
        }

        [Test]
        public void UpdateEstado_DeProcesandoAEnviado_DebeTenerExito()
        {
            var venta = new Venta { Estado = EstadoPedido.Procesando };

            venta.UpdateEstado(EstadoPedido.Enviado);

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Enviado));
        }

        [Test]
        public void UpdateEstado_DeEnviadoAEntregado_DebeTenerExito()
        {
            var venta = new Venta { Estado = EstadoPedido.Enviado };

            venta.UpdateEstado(EstadoPedido.Entregado);

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Entregado));
        }

        [Test]
        public void UpdateEstado_DePendienteACancelado_DebeTenerExito()
        {
            var venta = new Venta { Estado = EstadoPedido.Pendiente };

            venta.UpdateEstado(EstadoPedido.Cancelado);

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Cancelado));
        }

        // ==========================================
        // UpdateEstado - Transiciones NO permitidas
        // ==========================================

        [Test]
        public void UpdateEstado_DesdeCancelado_DebeLanzarExcepcion()
        {
            var venta = new Venta { Estado = EstadoPedido.Cancelado };

            Assert.Throws<InvalidOperationException>(() => venta.UpdateEstado(EstadoPedido.Enviado));
        }

        [Test]
        public void UpdateEstado_DesdeCanceladoAPendiente_DebeLanzarExcepcion()
        {
            var venta = new Venta { Estado = EstadoPedido.Cancelado };

            Assert.Throws<InvalidOperationException>(() => venta.UpdateEstado(EstadoPedido.Pendiente));
        }

        [Test]
        public void UpdateEstado_DesdeEntregadoAPendiente_DebeLanzarExcepcion()
        {
            var venta = new Venta { Estado = EstadoPedido.Entregado };

            Assert.Throws<InvalidOperationException>(() => venta.UpdateEstado(EstadoPedido.Pendiente));
        }

        [Test]
        public void UpdateEstado_DesdeEntregadoAEntregado_NoDaError()
        {
            var venta = new Venta { Estado = EstadoPedido.Entregado };

            // Entregado -> Entregado no debería fallar (no cambia de estado)
            venta.UpdateEstado(EstadoPedido.Entregado);

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Entregado));
        }

        // ==========================================
        // Valores por defecto
        // ==========================================

        [Test]
        public void Venta_PorDefecto_EstadoDebeSePendiente()
        {
            var venta = new Venta();

            Assert.That(venta.Estado, Is.EqualTo(EstadoPedido.Pendiente));
        }

        [Test]
        public void Venta_PorDefecto_LineasDebeSerColeccionVacia()
        {
            var venta = new Venta();

            Assert.That(venta.Lineas, Is.Not.Null);
            Assert.That(venta.Lineas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Venta_PorDefecto_FechaCompraDebeSerUtcNow()
        {
            var antes = DateTime.UtcNow;
            var venta = new Venta();
            var despues = DateTime.UtcNow;

            Assert.That(venta.FechaCompra, Is.InRange(antes, despues));
        }
    }
}
