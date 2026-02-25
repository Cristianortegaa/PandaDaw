using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Carrito;
using PandaBack.Services;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class CarritoModel : PageModel
{
    private readonly ICarritoService _carritoService;
    private readonly NotificacionService _notificacionService;

    public CarritoModel(ICarritoService carritoService, NotificacionService notificacionService)
    {
        _carritoService = carritoService;
        _notificacionService = notificacionService;
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

        // Actualizar contador carrito en tiempo real
        var carritoResult = await _carritoService.GetCarritoByUserIdAsync(UserId);
        if (carritoResult.IsSuccess)
        {
            _notificacionService.NotificarCarritoActualizado(UserId, carritoResult.Value.TotalItems);
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

        // Notificación de producto eliminado
        _notificacionService.Enviar(UserId, new Notificacion
        {
            Tipo = "info",
            Titulo = "Producto eliminado",
            Mensaje = "Se ha eliminado el producto del carrito",
            Icono = "fa-solid fa-trash"
        });

        // Actualizar contador
        var carritoResult = await _carritoService.GetCarritoByUserIdAsync(UserId);
        if (carritoResult.IsSuccess)
        {
            _notificacionService.NotificarCarritoActualizado(UserId, carritoResult.Value.TotalItems);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVaciarAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.VaciarCarritoAsync(UserId);

        // Notificación de carrito vaciado
        _notificacionService.Enviar(UserId, new Notificacion
        {
            Tipo = "info",
            Titulo = "Carrito vaciado",
            Mensaje = "Se han eliminado todos los productos del carrito",
            Icono = "fa-solid fa-broom"
        });
        _notificacionService.NotificarCarritoActualizado(UserId, 0);

        return RedirectToPage();
    }
}