using MailKit.Net.Smtp;
using MimeKit;
using PandaBack.Dtos.Ventas;

namespace PandaBack.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendConfirmacionPagoAsync(string toEmail, string nombreCliente, VentaResponseDto venta, byte[] facturaPdf)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var smtpUser = _config["Email:SmtpUser"] ?? "";
        var smtpPass = _config["Email:SmtpPass"] ?? "";
        var fromName = _config["Email:FromName"] ?? "PandaDaw";
        var fromEmail = _config["Email:FromEmail"] ?? smtpUser;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(nombreCliente, toEmail));
        message.Subject = $"PandaDaw - Confirmación de pedido #{venta.Id}";

        var subtotal = venta.Lineas.Sum(l => l.Subtotal);
        var iva = subtotal * 0.21m;
        var total = subtotal + iva;

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f8f9fa; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #059669, #10b981); padding: 32px; text-align: center; }}
        .header h1 {{ color: white; margin: 0; font-size: 24px; }}
        .header p {{ color: rgba(255,255,255,0.85); margin: 8px 0 0; font-size: 14px; }}
        .content {{ padding: 32px; }}
        .success-icon {{ width: 64px; height: 64px; background: #d1fae5; border-radius: 50%; margin: 0 auto 16px; display: flex; align-items: center; justify-content: center; font-size: 28px; }}
        .order-info {{ background: #f8f9fa; border-radius: 12px; padding: 20px; margin: 20px 0; }}
        .order-info h3 {{ margin: 0 0 12px; color: #1f2937; font-size: 14px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ text-align: left; padding: 10px 12px; background: #f1f5f9; font-size: 12px; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }}
        td {{ padding: 10px 12px; border-bottom: 1px solid #f1f5f9; font-size: 13px; color: #374151; }}
        .total-row {{ font-weight: bold; font-size: 15px; color: #059669; }}
        .footer {{ text-align: center; padding: 24px; color: #9ca3af; font-size: 12px; border-top: 1px solid #f1f5f9; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🐼 PandaDaw</h1>
            <p>Confirmación de pedido</p>
        </div>
        <div class='content'>
            <div style='text-align: center; margin-bottom: 24px;'>
                <div class='success-icon'>✅</div>
                <h2 style='margin: 0; color: #1f2937;'>¡Pago realizado con éxito!</h2>
                <p style='color: #6b7280; margin: 8px 0 0;'>Hola {nombreCliente}, tu pedido ha sido confirmado.</p>
            </div>
            
            <div class='order-info'>
                <h3>📦 Detalle del pedido #{venta.Id}</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Producto</th>
                            <th style='text-align:center;'>Cant.</th>
                            <th style='text-align:right;'>Precio</th>
                            <th style='text-align:right;'>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", venta.Lineas.Select(l => $@"
                        <tr>
                            <td>{l.ProductoNombre}</td>
                            <td style='text-align:center;'>{l.Cantidad}</td>
                            <td style='text-align:right;'>{l.PrecioUnitario:C}</td>
                            <td style='text-align:right;'>{l.Subtotal:C}</td>
                        </tr>"))}
                        <tr>
                            <td colspan='3' style='text-align:right; color: #6b7280;'>Subtotal</td>
                            <td style='text-align:right;'>{subtotal:C}</td>
                        </tr>
                        <tr>
                            <td colspan='3' style='text-align:right; color: #6b7280;'>IVA (21%)</td>
                            <td style='text-align:right;'>{iva:C}</td>
                        </tr>
                        <tr class='total-row'>
                            <td colspan='3' style='text-align:right;'>Total</td>
                            <td style='text-align:right;'>{total:C}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <p style='color: #6b7280; font-size: 13px; text-align: center;'>
                Adjuntamos la factura en formato PDF para tus registros.
            </p>
        </div>
        <div class='footer'>
            <p>PandaDaw © 2026 — Gracias por tu compra</p>
            <p>Este email es una confirmación automática de tu pedido.</p>
        </div>
    </div>
</body>
</html>";

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        // Adjuntar factura PDF
        builder.Attachments.Add($"Factura_PandaDaw_{venta.Id:D6}.pdf", facturaPdf, new ContentType("application", "pdf"));

        message.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email de confirmación enviado a {Email} para pedido #{VentaId}", toEmail, venta.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de confirmación a {Email} para pedido #{VentaId}", toEmail, venta.Id);
            // No lanzamos excepción para no bloquear el flujo de pago
        }
    }
}
