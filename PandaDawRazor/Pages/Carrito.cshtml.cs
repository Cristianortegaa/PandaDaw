using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Carrito;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class CarritoModel : PageModel
{
    private readonly ICarritoService _carritoService;

    public CarritoModel(ICarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    public CarritoDto? Carrito { get; set; }
    
    public long? UserId => long.TryParse(HttpContext.Session.GetString("UserId"), out var id) ? id : null;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!UserId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        var result = await _carritoService.GetCarritoByUserIdAsync(UserId.Value);
        if (result.IsSuccess)
        {
            Carrito = result.Value;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateCantidadAsync(long productoId, int cantidad)
    {
        if (!UserId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        if (cantidad <= 0)
        {
            await _carritoService.RemoveLineaCarritoAsync(UserId.Value, productoId);
        }
        else
        {
            await _carritoService.UpdateLineaCantidadAsync(UserId.Value, productoId, cantidad);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveLineaAsync(long productoId)
    {
        if (!UserId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.RemoveLineaCarritoAsync(UserId.Value, productoId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVaciarAsync()
    {
        if (!UserId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.VaciarCarritoAsync(UserId.Value);
        return RedirectToPage();
    }
}