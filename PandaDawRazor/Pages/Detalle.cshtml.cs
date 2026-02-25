using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Favoritos;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Models;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class DetalleModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly ICarritoService _carritoService;
    private readonly IFavoritoService _favoritoService;
    private readonly IValoracionService _valoracionService;

    public DetalleModel(
        IProductoService productoService,
        ICarritoService carritoService,
        IFavoritoService favoritoService,
        IValoracionService valoracionService)
    {
        _productoService = productoService;
        _carritoService = carritoService;
        _favoritoService = favoritoService;
        _valoracionService = valoracionService;
    }

    public Producto? Producto { get; set; }
    public List<ValoracionResponseDto> Valoraciones { get; set; } = new();
    public bool EsFavorito { get; set; }
    public long? FavoritoId { get; set; }
    
    public string? UserId => HttpContext.Session.GetString("UserId");
    public bool EstaAutenticado => !string.IsNullOrEmpty(UserId);
    
    [BindProperty]
    public int Estrellas { get; set; }
    
    [BindProperty]
    public string Resena { get; set; } = string.Empty;
    
    public string? MensajeError { get; set; }
    public string? MensajeExito { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var result = await _productoService.GetProductoByIdAsync(id);
        
        if (result.IsFailure)
        {
            return RedirectToPage("/Index");
        }
        
        Producto = result.Value;

        var valResult = await _valoracionService.GetValoracionesByProductoAsync(id);
        if (valResult.IsSuccess)
        {
            Valoraciones = valResult.Value.ToList();
        }

        if (!string.IsNullOrEmpty(UserId))
        {
            var favResult = await _favoritoService.GetUserFavoritosAsync(UserId);
            if (favResult.IsSuccess)
            {
                var fav = favResult.Value.FirstOrDefault(f => f.ProductoId == id);
                if (fav != null)
                {
                    EsFavorito = true;
                    FavoritoId = fav.Id;
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(long productoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        await _carritoService.AddLineaCarritoAsync(UserId, productoId, 1);
        
        return RedirectToPage("/Detalle", new { id = productoId });
    }

    public async Task<IActionResult> OnPostToggleFavoritoAsync(long productoId, long? favoritoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        if (favoritoId.HasValue)
        {
            await _favoritoService.RemoveFromFavoritosAsync(favoritoId.Value, UserId);
        }
        else
        {
            await _favoritoService.AddToFavoritosAsync(UserId, new CreateFavoritoDto { ProductoId = productoId });
        }

        return RedirectToPage("/Detalle", new { id = productoId });
    }

    public async Task<IActionResult> OnPostAddValoracionAsync(long productoId)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        if (Estrellas < 1 || Estrellas > 5)
        {
            MensajeError = "La puntuación debe estar entre 1 y 5";
            await LoadProductoAsync(productoId);
            return Page();
        }

        var valoracion = new CreateValoracionDto
        {
            ProductoId = productoId,
            Estrellas = Estrellas,
            Resena = Resena
        };

        var result = await _valoracionService.CreateValoracionAsync(UserId, valoracion);
        
        if (result.IsSuccess)
        {
            MensajeExito = "¡Valoración añadida correctamente!";
        }
        else
        {
            MensajeError = result.Error.Message;
        }

        return RedirectToPage("/Detalle", new { id = productoId });
    }

    private async Task LoadProductoAsync(long productoId)
    {
        var result = await _productoService.GetProductoByIdAsync(productoId);
        if (result.IsSuccess)
        {
            Producto = result.Value;
        }
        
        var valResult = await _valoracionService.GetValoracionesByProductoAsync(productoId);
        if (valResult.IsSuccess)
        {
            Valoraciones = valResult.Value.ToList();
        }
    }
}
