using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Services;

namespace PandaDawRazor.Filters;

/// <summary>
/// Inyecta contadores de carrito y favoritos en ViewData para el Layout.
/// </summary>
public class NavBadgePageFilter(ICarritoService carritoService, IFavoritoService favoritoService) : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HandlerInstance is PageModel page)
        {
            var userId = context.HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var carritoTask = carritoService.GetCarritoByUserIdAsync(userId);
                    var favoritosTask = favoritoService.GetUserFavoritosAsync(userId);
                    await Task.WhenAll(carritoTask, favoritosTask);

                    var carritoResult = await carritoTask;
                    var favoritosResult = await favoritosTask;

                    page.ViewData["CartCount"] = carritoResult.IsSuccess ? carritoResult.Value.TotalItems : 0;
                    page.ViewData["FavCount"] = favoritosResult.IsSuccess ? favoritosResult.Value.Count() : 0;
                }
                catch
                {
                    page.ViewData["CartCount"] = 0;
                    page.ViewData["FavCount"] = 0;
                }
            }
            else
            {
                page.ViewData["CartCount"] = 0;
                page.ViewData["FavCount"] = 0;
            }
        }

        await next();
    }
}
