using PandaBack.Dtos.Ventas;

namespace PandaBack.Services.Factura;

public interface IFacturaService
{
    byte[] GenerarFacturaPdf(VentaResponseDto venta);
}
