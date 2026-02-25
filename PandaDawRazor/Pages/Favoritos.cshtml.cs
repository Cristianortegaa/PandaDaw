using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Favoritos;
using PandaBack.Services;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class FavoritosModel : PageModel
{
    private readonly IFavoritoService _favoritoService;
    private readonly ICarritoService _carritoService;
    private readonly IProductoService _productoService;
    private readonly NotificacionService _notificacionService;

    public FavoritosModel(IFavoritoService favoritoService, ICarritoService carritoService, IProductoService productoService, NotificacionService notificacionService)
    {
        _favoritoService = favoritoService;
        _carritoService = carritoService;
        _productoService = productoService;
        _notificacionService = notificacionService;
    }

    public List<FavoritoResponseDto> Favoritos { get; set; } = new();
    public string? UserId => HttpContext.Session.GetString("UserId");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _favoritoService.GetUserFavoritosAsync(UserId);
        if (result.IsSuccess)
        {
            Favoritos = result.Value.ToList();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveFavoritoAsync(long favoritoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _favoritoService.RemoveFromFavoritosAsync(favoritoId, UserId);
        return RedirectToPage("/Favoritos");
    }

    public async Task<IActionResult> OnPostAddToCartAsync(long productoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.AddLineaCarritoAsync(UserId, productoId, 1);

        // Notificaciones
        var productoResult = await _productoService.GetProductoByIdAsync(productoId);
        if (productoResult.IsSuccess)
        {
            var prod = productoResult.Value;
            _notificacionService.Enviar(UserId, new Notificacion
            {
                Tipo = "success",
                Titulo = "¡Añadido al carrito!",
                Mensaje = $"{prod.Nombre} se ha añadido desde favoritos",
                Icono = "fa-solid fa-cart-plus"
            });

            if (prod.Stock > 0 && prod.Stock <= 5)
            {
                _notificacionService.EnviarATodos(new Notificacion
                {
                    Tipo = "warning",
                    Titulo = "¡Quedan pocas unidades!",
                    Mensaje = $"Solo quedan {prod.Stock} unidades de {prod.Nombre}",
                    Icono = "fa-solid fa-triangle-exclamation"
                });
            }
        }

        var carritoResult = await _carritoService.GetCarritoByUserIdAsync(UserId);
        if (carritoResult.IsSuccess)
        {
            _notificacionService.NotificarCarritoActualizado(UserId, carritoResult.Value.TotalItems);
        }

        return RedirectToPage("/Favoritos");
    }
}
