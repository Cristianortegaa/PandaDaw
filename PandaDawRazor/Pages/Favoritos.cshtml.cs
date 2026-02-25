using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Favoritos;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class FavoritosModel : PageModel
{
    private readonly IFavoritoService _favoritoService;
    private readonly ICarritoService _carritoService;

    public FavoritosModel(IFavoritoService favoritoService, ICarritoService carritoService)
    {
        _favoritoService = favoritoService;
        _carritoService = carritoService;
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
        return RedirectToPage("/Favoritos");
    }
}
