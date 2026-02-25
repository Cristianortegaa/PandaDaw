using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Ventas;
using PandaBack.Services;

namespace PandaDawRazor.Pages;

public class PedidosModel : PageModel
{
    private readonly IVentaService _ventaService;

    public PedidosModel(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    public List<VentaResponseDto> Pedidos { get; set; } = new();
    public string? UserId => HttpContext.Session.GetString("UserId");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _ventaService.GetVentasByUserAsync(UserId);
        if (result.IsSuccess)
        {
            Pedidos = result.Value.ToList();
        }
        return Page();
    }
}
