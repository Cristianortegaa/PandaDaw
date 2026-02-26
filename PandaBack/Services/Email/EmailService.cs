using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PandaBack.Dtos.Ventas;

namespace PandaBack.Services.Email;

/// <summary>
/// Servicio de email usando MailKit.
/// Envía la factura como PDF adjunto al correo del cliente.
/// Compatible con cualquier servidor SMTP (Gmail, Mailtrap, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendConfirmacionPagoAsync(string toEmail, string nombreCliente, VentaResponseDto venta, byte[] facturaPdf)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var fromName = smtpSettings["FromName"] ?? "PandaDaw";
        var fromEmail = smtpSettings["FromEmail"] ?? "no-reply@pandadaw.es";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(nombreCliente, toEmail));
        message.Subject = $"PandaDaw — Confirmación de pedido #{venta.Id}";

        var builder = new BodyBuilder
        {
            HtmlBody = GenerarHtmlEmail(venta, nombreCliente)
        };

        // Adjuntar factura PDF
        builder.Attachments.Add(
            $"Factura_PandaDaw_{venta.Id:D6}.pdf",
            facturaPdf,
            ContentType.Parse("application/pdf"));

        message.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();

            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var useSsl = bool.Parse(smtpSettings["UseSsl"] ?? "true");
            var username = smtpSettings["Username"] ?? "";
            var password = smtpSettings["Password"] ?? "";

            var secureOption = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureOption);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Email de confirmación enviado a {Email} para pedido #{VentaId}",
                toEmail, venta.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar email de confirmación a {Email} para pedido #{VentaId}",
                toEmail, venta.Id);
            // No lanzamos excepción para que el flujo de pago no falle por un error de email
        }
    }

    private static string GenerarHtmlEmail(VentaResponseDto venta, string nombreCliente)
    {
        // Los precios ya incluyen IVA → desglosamos
        var total = venta.Lineas.Sum(l => l.Subtotal);
        var baseImponible = Math.Round(total / 1.21m, 2);
        var ivaIncluido = total - baseImponible;
        var fechaCompra = venta.FechaCompra.ToString("dd/MM/yyyy HH:mm");

        var lineasHtml = string.Join("", venta.Lineas.Select(l => $$"""
                        <tr>
                            <td>{{l.ProductoNombre}}</td>
                            <td style="text-align:center;">{{l.Cantidad}}</td>
                            <td style="text-align:right;">{{l.PrecioUnitario:C}}</td>
                            <td style="text-align:right;">{{l.Subtotal:C}}</td>
                        </tr>
        """));

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }
                .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
                .header { background: linear-gradient(135deg, #059669, #10b981); color: white; padding: 30px; text-align: center; }
                .header h1 { margin: 0; font-size: 28px; }
                .header .subtitle { color: rgba(255,255,255,0.85); font-size: 14px; margin-top: 5px; }
                .content { padding: 30px; }
                .success { background: #e8f5e9; border-left: 4px solid #4caf50; padding: 15px; margin-bottom: 20px; border-radius: 4px; }
                .success h2 { color: #2e7d32; margin: 0 0 5px 0; font-size: 18px; }
                .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }
                .info-item { background: #f5f5f5; padding: 12px; border-radius: 8px; }
                .info-item .label { font-size: 11px; color: #666; text-transform: uppercase; letter-spacing: 0.5px; }
                .info-item .value { font-size: 15px; font-weight: 600; color: #059669; margin-top: 4px; }
                .order-info { background: #f8f9fa; border-radius: 12px; padding: 20px; margin: 20px 0; }
                .order-info h3 { margin: 0 0 12px; color: #1f2937; font-size: 14px; }
                table { width: 100%; border-collapse: collapse; }
                th { text-align: left; padding: 10px 12px; background: #f1f5f9; font-size: 12px; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
                td { padding: 10px 12px; border-bottom: 1px solid #f1f5f9; font-size: 13px; color: #374151; }
                .total-row { font-weight: bold; font-size: 15px; color: #059669; }
                .note { font-size: 13px; color: #666; margin-top: 20px; padding: 15px; background: #fff3e0; border-radius: 8px; }
                .footer { background: #263238; color: #90a4ae; padding: 20px; text-align: center; font-size: 12px; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">
                    <h1>🐼 PandaDaw</h1>
                    <div class="subtitle">Tu tienda de confianza</div>
                </div>
                <div class="content">
                    <div class="success">
                        <h2>✓ Pago confirmado</h2>
                        <p style="margin:0; color:#333;">Hola {{nombreCliente}}, tu pedido ha sido registrado correctamente.</p>
                    </div>

                    <div class="info-grid">
                        <div class="info-item">
                            <div class="label">Nº Pedido</div>
                            <div class="value">#{{venta.Id}}</div>
                        </div>
                        <div class="info-item">
                            <div class="label">Fecha</div>
                            <div class="value">{{fechaCompra}}</div>
                        </div>
                        <div class="info-item">
                            <div class="label">Total (IVA incluido)</div>
                            <div class="value">{{total:C}}</div>
                        </div>
                        <div class="info-item">
                            <div class="label">Estado</div>
                            <div class="value">{{venta.Estado}}</div>
                        </div>
                    </div>

                    <div class="order-info">
                        <h3>📦 Detalle del pedido</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Producto</th>
                                    <th style="text-align:center;">Cant.</th>
                                    <th style="text-align:right;">Precio</th>
                                    <th style="text-align:right;">Subtotal</th>
                                </tr>
                            </thead>
                            <tbody>
                                {{lineasHtml}}
                                <tr>
                                    <td colspan="3" style="text-align:right; color: #6b7280;">Base imponible</td>
                                    <td style="text-align:right;">{{baseImponible:C}}</td>
                                </tr>
                                <tr>
                                    <td colspan="3" style="text-align:right; color: #6b7280;">IVA (21%) incluido</td>
                                    <td style="text-align:right;">{{ivaIncluido:C}}</td>
                                </tr>
                                <tr class="total-row">
                                    <td colspan="3" style="text-align:right;">Total</td>
                                    <td style="text-align:right;">{{total:C}}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div class="note">
                        📎 Adjuntamos la <strong>factura</strong> en formato PDF para tus registros.
                    </div>
                </div>
                <div class="footer">
                    <p>© 2026 PandaDaw — Todos los derechos reservados</p>
                    <p>Este email es una notificación automática, por favor no responda a este mensaje.</p>
                </div>
            </div>
        </body>
        </html>
        """;
    }
}
