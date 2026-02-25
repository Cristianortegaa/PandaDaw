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
        }
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateCantidadAsync(long productoId, int cantidad)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        if (cantidad <= 0)
        {
            await _carritoService.RemoveLineaCarritoAsync(UserId, productoId);
        }
        else
        {
            await _carritoService.UpdateLineaCantidadAsync(UserId, productoId, cantidad);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveLineaAsync(long productoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.RemoveLineaCarritoAsync(UserId, productoId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVaciarAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.VaciarCarritoAsync(UserId);
        return RedirectToPage();
    }
}