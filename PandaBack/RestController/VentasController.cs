using System.Security.Claims;
using CSharpFunctionalExtensions;
using PandaBack.Dtos.Ventas;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.Services;
using PandaBack.Services.Factura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para la gestión de ventas (pedidos).
/// </summary>
/// <remarks>
/// Este controlador gestiona la creación de ventas a partir del carrito,
/// la consulta de pedidos del usuario y la actualización de estados por parte del administrador.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class VentasController(IVentaService service, IFacturaService facturaService) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las ventas del sistema (solo administradores).
    /// </summary>
    /// <returns>Lista de todas las ventas.</returns>
    /// <response code="200">Devuelve la lista completa de ventas.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="403">Si el usuario no tiene rol de administrador.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<VentaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync()
    {
        return await service.GetAllVentasAsync().Match(
            onSuccess: ventas => Ok(ventas),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Obtiene las ventas (pedidos) del usuario autenticado.
    /// </summary>
    /// <returns>Lista de ventas del usuario.</returns>
    /// <response code="200">Devuelve las ventas del usuario.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("mis-pedidos")]
    [ProducesResponseType(typeof(IEnumerable<VentaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyOrdersAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.GetVentasByUserAsync(userId).Match(
            onSuccess: ventas => Ok(ventas),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Obtiene una venta específica por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <returns>La venta encontrada.</returns>
    /// <response code="200">Devuelve la venta encontrada.</response>
    /// <response code="404">Si la venta no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(VentaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(long id)
    {
        return await service.GetVentaByIdAsync(id).Match(
            onSuccess: venta => Ok(venta),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Descarga la factura en PDF de una venta específica.
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <returns>Archivo PDF con la factura.</returns>
    /// <response code="200">Devuelve el PDF de la factura.</response>
    /// <response code="403">Si la venta no pertenece al usuario autenticado.</response>
    /// <response code="404">Si la venta no existe.</response>
    [HttpGet("{id:long}/factura")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarFacturaAsync(long id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var isAdmin = User.IsInRole("Admin");

        var result = await service.GetVentaByIdAsync(id);
        if (result.IsFailure)
        {
            return result.Error switch
            {
                NotFoundError => NotFound(new { message = result.Error.Message }),
                _ => StatusCode(500, new { message = result.Error.Message })
            };
        }

        var venta = result.Value;

        // Solo el dueño o un admin puede descargar la factura
        if (venta.UsuarioId != userId && !isAdmin)
            return Forbid();

        var pdfBytes = facturaService.GenerarFacturaPdf(venta);
        return File(pdfBytes, "application/pdf", $"Factura_PandaDaw_{id:D6}.pdf");
    }

    /// <summary>
    /// Crea una nueva venta a partir del carrito del usuario autenticado.
    /// </summary>
    /// <returns>La venta creada.</returns>
    /// <response code="201">La venta se creó correctamente.</response>
    /// <response code="400">Si el carrito está vacío o hay stock insuficiente.</response>
    /// <response code="404">Si algún producto del carrito no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VentaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFromCarritoAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.CreateVentaFromCarritoAsync(userId).Match(
            onSuccess: venta => Created($"/api/Ventas/{venta.Id}", venta),
            onFailure: error => error switch
            {
                CarritoVacioError => BadRequest(new { message = error.Message }),
                StockInsuficienteError => BadRequest(new { message = error.Message }),
                NotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Actualiza el estado de una venta (solo administradores).
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <param name="nuevoEstado">Nuevo estado del pedido (Pendiente, Procesando, Enviado, Entregado, Cancelado).</param>
    /// <returns>La venta con el estado actualizado.</returns>
    /// <response code="200">El estado se actualizó correctamente.</response>
    /// <response code="400">Si el cambio de estado no está permitido.</response>
    /// <response code="404">Si la venta no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="403">Si el usuario no tiene rol de administrador.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPatch("{id:long}/estado")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VentaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEstadoAsync(long id, [FromQuery] EstadoPedido nuevoEstado)
    {
        return await service.UpdateEstadoVentaAsync(id, nuevoEstado).Match(
            onSuccess: venta => Ok(venta),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                OperacionNoPermitidaError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }
}
