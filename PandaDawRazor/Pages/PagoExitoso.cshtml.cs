using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Ventas;
using PandaBack.Services;
using PandaBack.Services.Email;
using PandaBack.Services.Factura;
using PandaBack.Services.Stripe;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class PagoExitosoModel : PageModel
{
    private readonly IStripeService _stripeService;
    private readonly IVentaService _ventaService;
    private readonly IFacturaService _facturaService;
    private readonly IEmailService _emailService;
    private readonly NotificacionService _notificacionService;
    private readonly ILogger<PagoExitosoModel> _logger;

    public PagoExitosoModel(
        IStripeService stripeService,
        IVentaService ventaService,
        IFacturaService facturaService,
        IEmailService emailService,
        NotificacionService notificacionService,
        ILogger<PagoExitosoModel> logger)
    {
        _stripeService = stripeService;
        _ventaService = ventaService;
        _facturaService = facturaService;
        _emailService = emailService;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    public VentaResponseDto? VentaCreada { get; set; }
    public bool PagoExitoso { get; set; }
    public string? ErrorMessage { get; set; }

    public string? UserId => HttpContext.Session.GetString("UserId");

    public async Task<IActionResult> OnGetAsync(string? session_id)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        if (string.IsNullOrEmpty(session_id))
        {
            return RedirectToPage("/Carrito");
        }

        // Protección contra doble ejecución (refresh del navegador)
        var processedKey = $"stripe_processed_{session_id}";
        var processedVentaId = HttpContext.Session.GetString(processedKey);
        if (!string.IsNullOrEmpty(processedVentaId))
        {
            // Ya procesamos este session_id, recuperar la venta existente
            if (long.TryParse(processedVentaId, out var ventaId))
            {
                var ventaResult = await _ventaService.GetVentaByIdAsync(ventaId);
                if (ventaResult.IsSuccess)
                {
                    PagoExitoso = true;
                    VentaCreada = ventaResult.Value;
                    ViewData["CartCount"] = 0;
                    return Page();
                }
            }
        }

        // Verificar que el pago fue exitoso en Stripe
        var statusResult = await _stripeService.GetSessionPaymentStatusAsync(session_id);
        if (statusResult.IsFailure)
        {
            ErrorMessage = "No se pudo verificar el estado del pago.";
            return Page();
        }

        if (statusResult.Value != "paid")
        {
            ErrorMessage = "El pago no se ha completado. Por favor, inténtalo de nuevo.";
            return Page();
        }

        // Crear la venta a partir del carrito
        var ventaCreateResult = await _ventaService.CreateVentaFromCarritoAsync(UserId);
        if (ventaCreateResult.IsSuccess)
        {
            PagoExitoso = true;
            VentaCreada = ventaCreateResult.Value;

            // Guardar en sesión que este session_id ya fue procesado
            HttpContext.Session.SetString(processedKey, VentaCreada.Id.ToString());

            // Actualizar badge del carrito a 0 (la venta ya se creó y el carrito se vació)
            ViewData["CartCount"] = 0;
            _notificacionService.NotificarCarritoActualizado(UserId, 0);
            _notificacionService.Enviar(UserId, new Notificacion
            {
                Tipo = "success",
                Titulo = "¡Pago exitoso!",
                Mensaje = "Tu pago ha sido procesado correctamente",
                Icono = "fa-solid fa-check-circle"
            });

            // Generar factura PDF y enviar email de confirmación
            try
            {
                var facturaPdf = _facturaService.GenerarFacturaPdf(VentaCreada);
                await _emailService.SendConfirmacionPagoAsync(
                    VentaCreada.UsuarioEmail,
                    VentaCreada.UsuarioNombre,
                    VentaCreada,
                    facturaPdf
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura o enviar email para venta #{VentaId}", VentaCreada.Id);
                // No bloqueamos el flujo por un error de email/factura
            }
        }
        else
        {
            ErrorMessage = ventaCreateResult.Error.Message;
        }

        return Page();
    }
}
