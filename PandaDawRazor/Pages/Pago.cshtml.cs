using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Carrito;
using PandaBack.Dtos.Ventas;
using PandaBack.Services;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class PagoModel : PageModel
{
    private readonly ICarritoService _carritoService;
    private readonly IVentaService _ventaService;
    private readonly NotificacionService _notificacionService;

    public PagoModel(ICarritoService carritoService, IVentaService ventaService, NotificacionService notificacionService)
    {
        _carritoService = carritoService;
        _ventaService = ventaService;
        _notificacionService = notificacionService;
    }

    public CarritoDto? Carrito { get; set; }
    public bool PagoExitoso { get; set; }
    public VentaResponseDto? VentaCreada { get; set; }
    public string? ErrorMessage { get; set; }
    
    public string? UserId => HttpContext.Session.GetString("UserId");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _carritoService.GetCarritoByUserIdAsync(UserId);
        if (result.IsSuccess)
        {
            Carrito = result.Value;
            if (Carrito == null || !Carrito.Lineas.Any())
            {
                return RedirectToPage("/Carrito");
            }
        }
        else
        {
            return RedirectToPage("/Carrito");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmarPagoAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        // Construir las URLs de éxito y cancelación para Stripe
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var successUrl = $"{baseUrl}/PagoExitoso";
        var cancelUrl = $"{baseUrl}/Pago";

        var stripeResult = await _stripeService.CreateCheckoutSessionAsync(UserId, successUrl, cancelUrl);

        if (stripeResult.IsSuccess)
        {
            PagoExitoso = true;
            VentaCreada = ventaResult.Value;
            Carrito = new CarritoDto();

            // Alerta: Pedido creado exitosamente
            _notificacionService.Enviar(UserId, new Notificacion
            {
                Tipo = "success",
                Titulo = "¡Pedido confirmado!",
                Mensaje = $"Tu pedido #{VentaCreada.Id} ha sido creado por {VentaCreada.Total:C}",
                Icono = "fa-solid fa-check-circle"
            });

            // Actualizar carrito a 0
            _notificacionService.NotificarCarritoActualizado(UserId, 0);
        }
        else
        {
            ErrorMessage = stripeResult.Error.Message;
            var carritoResult = await _carritoService.GetCarritoByUserIdAsync(UserId);
            if (carritoResult.IsSuccess)
            {
                Carrito = carritoResult.Value;
            }
            return Page();
        }
    }
}