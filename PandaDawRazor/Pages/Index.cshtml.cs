using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Productos;
using PandaBack.Dtos.Favoritos;
using PandaBack.Models;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class IndexModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly ICarritoService _carritoService;
    private readonly IFavoritoService _favoritoService;

    public IndexModel(IProductoService productoService, ICarritoService carritoService, IFavoritoService favoritoService)
    {
        _productoService = productoService;
        _carritoService = carritoService;
        _favoritoService = favoritoService;
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
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var userId))
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
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.AddLineaCarritoAsync(userId, productoId, 1);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleFavoritoAsync(long productoId, long? favoritoId)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userId))
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