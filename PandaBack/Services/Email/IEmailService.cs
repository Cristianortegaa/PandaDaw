using PandaBack.Dtos.Ventas;

namespace PandaBack.Services.Email;

public interface IEmailService
{
    Task SendConfirmacionPagoAsync(string toEmail, string nombreCliente, VentaResponseDto venta, byte[] facturaPdf);
}
