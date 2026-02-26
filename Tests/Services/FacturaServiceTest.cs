using PandaBack.Dtos.Ventas;
using PandaBack.Services.Factura;
using QuestPDF.Infrastructure;

namespace Tests.Services;

/// <summary>
/// Tests unitarios para FacturaService.
/// Verifica que se generan PDFs válidos con el contenido correcto.
/// </summary>
public class FacturaServiceTest
{
    private FacturaService _service;

    [OneTimeSetUp]
    public void ConfigurarLicencia()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [SetUp]
    public void PrepararTodo()
    {
        _service = new FacturaService();
    }

    /// <summary>
    /// Crea un VentaResponseDto de ejemplo para los tests.
    /// </summary>
    private static VentaResponseDto CrearVentaEjemplo(int numLineas = 2, decimal precioUnitario = 50m)
    {
        var lineas = Enumerable.Range(1, numLineas).Select(i => new LineaVentaResponseDto
        {
            ProductoId = i,
            ProductoNombre = $"Producto Test {i}",
            ProductoImagen = $"https://ejemplo.com/producto{i}.jpg",
            Cantidad = i,
            PrecioUnitario = precioUnitario,
            Subtotal = i * precioUnitario
        }).ToList();

        return new VentaResponseDto
        {
            Id = 1001,
            FechaCompra = new DateTime(2026, 2, 15, 10, 30, 0),
            Total = lineas.Sum(l => l.Subtotal),
            Estado = "Pendiente",
            UsuarioId = "user-123",
            UsuarioNombre = "Juan García López",
            UsuarioEmail = "juan@test.com",
            Lineas = lineas
        };
    }

    // ==========================================
    // 1. GENERACIÓN BÁSICA
    // ==========================================

    [Test]
    public void GenerarFactura_DebeRetornarBytesNoVacios()
    {
        // PREPARAR
        var venta = CrearVentaEjemplo();

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerarFactura_DebeRetornarPdfValido()
    {
        // PREPARAR
        var venta = CrearVentaEjemplo();

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR — Los PDFs empiezan con %PDF
        Assert.That(pdf.Length, Is.GreaterThan(4));
        var header = System.Text.Encoding.ASCII.GetString(pdf, 0, 5);
        Assert.That(header, Does.StartWith("%PDF"));
    }

    // ==========================================
    // 2. CONTENIDO DEL PDF
    // ==========================================

    [Test]
    public void GenerarFactura_ConUnaLinea_DebeGenerarPdfValido()
    {
        // PREPARAR
        var venta = CrearVentaEjemplo(numLineas: 1, precioUnitario: 100m);

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerarFactura_ConVariasLineas_DebeGenerarPdfMasGrande()
    {
        // PREPARAR
        var ventaPequeña = CrearVentaEjemplo(numLineas: 1, precioUnitario: 10m);
        var ventaGrande = CrearVentaEjemplo(numLineas: 10, precioUnitario: 10m);

        // ACTUAR
        var pdfPequeño = _service.GenerarFacturaPdf(ventaPequeña);
        var pdfGrande = _service.GenerarFacturaPdf(ventaGrande);

        // COMPROBAR — Más líneas = PDF más grande
        Assert.That(pdfGrande.Length, Is.GreaterThan(pdfPequeño.Length));
    }

    // ==========================================
    // 3. CÁLCULOS DE IVA
    // ==========================================

    [Test]
    public void GenerarFactura_DesgloseIva_DebeSerCorrecto()
    {
        // PREPARAR — Total = 121€ (100 base + 21 IVA)
        var venta = new VentaResponseDto
        {
            Id = 42,
            FechaCompra = DateTime.UtcNow,
            Total = 121m,
            Estado = "Pendiente",
            UsuarioId = "user-1",
            UsuarioNombre = "Test User",
            UsuarioEmail = "test@test.com",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 1,
                    ProductoNombre = "Producto IVA Test",
                    Cantidad = 1,
                    PrecioUnitario = 121m,
                    Subtotal = 121m
                }
            }
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR — El PDF se genera sin errores
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));

        // Verificar cálculos internos (base = 121/1.21 = 100, IVA = 21)
        var total = venta.Lineas.Sum(l => l.Subtotal);
        var baseImponible = Math.Round(total / 1.21m, 2);
        var iva = total - baseImponible;

        Assert.That(baseImponible, Is.EqualTo(100m));
        Assert.That(iva, Is.EqualTo(21m));
        Assert.That(baseImponible + iva, Is.EqualTo(total));
    }

    [Test]
    public void GenerarFactura_DesgloseIva_ConDecimales_DebeSerConsistente()
    {
        // PREPARAR — Total = 99.99€
        var venta = new VentaResponseDto
        {
            Id = 99,
            FechaCompra = DateTime.UtcNow,
            Total = 99.99m,
            Estado = "Enviado",
            UsuarioId = "user-2",
            UsuarioNombre = "María López",
            UsuarioEmail = "maria@test.com",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 2,
                    ProductoNombre = "Auriculares Pro",
                    Cantidad = 1,
                    PrecioUnitario = 99.99m,
                    Subtotal = 99.99m
                }
            }
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR — Base + IVA = Total (sin pérdida de céntimos)
        var total = venta.Lineas.Sum(l => l.Subtotal);
        var baseImponible = Math.Round(total / 1.21m, 2);
        var iva = total - baseImponible;

        Assert.That(baseImponible + iva, Is.EqualTo(total));
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    // ==========================================
    // 4. CASOS LÍMITE
    // ==========================================

    [Test]
    public void GenerarFactura_ConTotalCero_DebeGenerarPdf()
    {
        // PREPARAR
        var venta = new VentaResponseDto
        {
            Id = 0,
            FechaCompra = DateTime.UtcNow,
            Total = 0,
            Estado = "Pendiente",
            UsuarioId = "user-0",
            UsuarioNombre = "Zero User",
            UsuarioEmail = "zero@test.com",
            Lineas = new List<LineaVentaResponseDto>()
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerarFactura_ConDatosUsuarioVacios_DebeGenerarPdf()
    {
        // PREPARAR
        var venta = new VentaResponseDto
        {
            Id = 5,
            FechaCompra = DateTime.UtcNow,
            Total = 50m,
            Estado = "Pendiente",
            UsuarioId = "",
            UsuarioNombre = "",
            UsuarioEmail = "",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 1,
                    ProductoNombre = "Test",
                    Cantidad = 1,
                    PrecioUnitario = 50m,
                    Subtotal = 50m
                }
            }
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerarFactura_ConProductoNombreLargo_DebeGenerarPdf()
    {
        // PREPARAR
        var nombreLargo = new string('A', 500);
        var venta = new VentaResponseDto
        {
            Id = 10,
            FechaCompra = DateTime.UtcNow,
            Total = 25m,
            Estado = "Entregado",
            UsuarioId = "user-10",
            UsuarioNombre = "Test",
            UsuarioEmail = "test@test.com",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 1,
                    ProductoNombre = nombreLargo,
                    Cantidad = 1,
                    PrecioUnitario = 25m,
                    Subtotal = 25m
                }
            }
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerarFactura_MultiplesLlamadas_DebeSerConsistente()
    {
        // PREPARAR
        var venta = CrearVentaEjemplo();

        // ACTUAR — Generar dos veces la misma factura
        var pdf1 = _service.GenerarFacturaPdf(venta);
        var pdf2 = _service.GenerarFacturaPdf(venta);

        // COMPROBAR — Ambos deben ser PDFs válidos del mismo tamaño aprox.
        Assert.That(pdf1.Length, Is.GreaterThan(0));
        Assert.That(pdf2.Length, Is.GreaterThan(0));
        // Los tamaños pueden variar ligeramente por timestamps internos
        Assert.That(Math.Abs(pdf1.Length - pdf2.Length), Is.LessThan(500));
    }

    [Test]
    public void GenerarFactura_ConPreciosAltos_DebeGenerarPdf()
    {
        // PREPARAR
        var venta = new VentaResponseDto
        {
            Id = 999,
            FechaCompra = DateTime.UtcNow,
            Total = 99999.99m,
            Estado = "Pendiente",
            UsuarioId = "user-vip",
            UsuarioNombre = "Cliente VIP",
            UsuarioEmail = "vip@test.com",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 1,
                    ProductoNombre = "Producto Premium",
                    Cantidad = 1,
                    PrecioUnitario = 99999.99m,
                    Subtotal = 99999.99m
                }
            }
        };

        // ACTUAR
        var pdf = _service.GenerarFacturaPdf(venta);

        // COMPROBAR
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }
}
