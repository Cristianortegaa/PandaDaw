using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Carrito;
using PandaBack.Dtos.Ventas;
using PandaBack.Services;
using PandaBack.Services.Stripe;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class PagoModel : PageModel
{
    private readonly ICarritoService _carritoService;
    private readonly IVentaService _ventaService;
    private readonly IStripeService _stripeService;
    private readonly NotificacionService _notificacionService;

    public PagoModel(ICarritoService carritoService, IVentaService ventaService, IStripeService stripeService, NotificacionService notificacionService)
    {
        _carritoService = carritoService;
        _ventaService = ventaService;
        _stripeService = stripeService;
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

        // Url.Page genera URLs absolutas bien formadas, evitando errores de Stripe "Not a valid URL"
        var successUrl = Url.Page("/PagoExitoso", pageHandler: null, values: null, protocol: Request.Scheme)!;
        var cancelUrl = Url.Page("/Pago", pageHandler: null, values: null, protocol: Request.Scheme)!;

        var stripeResult = await _stripeService.CreateCheckoutSessionAsync(UserId, successUrl, cancelUrl);

        if (stripeResult.IsSuccess)
        {
            // Redirigir al checkout de Stripe (la URL devuelta por la API)
            // El carrito se vaciará en PagoExitoso tras verificar el pago
            return Redirect(stripeResult.Value);
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