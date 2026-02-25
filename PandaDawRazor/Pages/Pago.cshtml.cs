using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Carrito;
using PandaBack.Dtos.Ventas;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class PagoModel : PageModel
{
    private readonly ICarritoService _carritoService;
    private readonly IVentaService _ventaService;

    public PagoModel(ICarritoService carritoService, IVentaService ventaService)
    {
        _carritoService = carritoService;
        _ventaService = ventaService;
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

        var ventaResult = await _ventaService.CreateVentaFromCarritoAsync(UserId);
        
        if (ventaResult.IsSuccess)
        {
            PagoExitoso = true;
            VentaCreada = ventaResult.Value;
            Carrito = new CarritoDto();
        }
        else
        {
            ErrorMessage = ventaResult.Error.Message;
            var carritoResult = await _carritoService.GetCarritoByUserIdAsync(UserId);
            if (carritoResult.IsSuccess)
            {
                Carrito = carritoResult.Value;
            }
        }

        return Page();
    }
}