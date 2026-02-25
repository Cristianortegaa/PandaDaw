using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Productos;
using PandaBack.Dtos.Favoritos;
using PandaBack.Models;
using PandaBack.Services;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class IndexModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly ICarritoService _carritoService;
    private readonly IFavoritoService _favoritoService;
    private readonly NotificacionService _notificacionService;

    public IndexModel(IProductoService productoService, ICarritoService carritoService, IFavoritoService favoritoService, NotificacionService notificacionService)
    {
        _productoService = productoService;
        _carritoService = carritoService;
        _favoritoService = favoritoService;
        _notificacionService = notificacionService;
    }

    public List<Producto> Productos { get; set; } = new();
    public Dictionary<long, long> FavoritosMap { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? Categoria { get; set; }

    public async Task OnGetAsync()
    {
        // Cargar productos
        if (!string.IsNullOrEmpty(Categoria) && Enum.TryParse<Categoria>(Categoria, out var cat))
        {
            var result = await _productoService.GetProductosByCategoryAsync(cat);
            if (result.IsSuccess)
            {
                Productos = result.Value.ToList();
            }
        }
        else
        {
            var result = await _productoService.GetAllProductosAsync();
            if (result.IsSuccess)
            {
                Productos = result.Value.ToList();
            }
        }

        // Filtrar por búsqueda si hay término
        if (!string.IsNullOrEmpty(Buscar))
        {
            Productos = Productos
                .Where(p => p.Nombre.Contains(Buscar, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Excluir productos eliminados
        Productos = Productos.Where(p => !p.IsDeleted).ToList();

        // Cargar favoritos del usuario si está autenticado
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var favResult = await _favoritoService.GetUserFavoritosAsync(userId);
            if (favResult.IsSuccess)
            {
                FavoritosMap = favResult.Value.ToDictionary(f => f.ProductoId, f => f.Id);
            }
        }
    }

    public async Task<IActionResult> OnPostAddToCartAsync(long productoId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.AddLineaCarritoAsync(userId, productoId, 1);

        // Obtener info del producto para notificaciones
        var productoResult = await _productoService.GetProductoByIdAsync(productoId);
        if (productoResult.IsSuccess)
        {
            var prod = productoResult.Value;

            // Alerta: Añadido al carrito
            _notificacionService.Enviar(userId, new Notificacion
            {
                Tipo = "success",
                Titulo = "¡Añadido al carrito!",
                Mensaje = $"{prod.Nombre} se ha añadido a tu carrito",
                Icono = "fa-solid fa-cart-plus"
            });

            // Alerta: Stock bajo
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

            if (prod.Stock <= 0)
            {
                _notificacionService.EnviarATodos(new Notificacion
                {
                    Tipo = "error",
                    Titulo = "Producto agotado",
                    Mensaje = $"{prod.Nombre} se ha agotado",
                    Icono = "fa-solid fa-box-open"
                });
            }
        }

        // Actualizar contador carrito en tiempo real
        var carritoResult = await _carritoService.GetCarritoByUserIdAsync(userId);
        if (carritoResult.IsSuccess)
        {
            _notificacionService.NotificarCarritoActualizado(userId, carritoResult.Value.TotalItems);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleFavoritoAsync(long productoId, long? favoritoId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        if (favoritoId.HasValue)
        {
            await _favoritoService.RemoveFromFavoritosAsync(favoritoId.Value, userId);
        }
        else
        {
            await _favoritoService.AddToFavoritosAsync(userId, new CreateFavoritoDto { ProductoId = productoId });
        }

        return RedirectToPage();
    }
}