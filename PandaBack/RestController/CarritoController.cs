using System.Security.Claims;
using CSharpFunctionalExtensions;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;
using PandaBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para la gestión del carrito de compras.
/// </summary>
/// <remarks>
/// Este controlador maneja las operaciones sobre el carrito del usuario autenticado:
/// consultar, agregar líneas, actualizar cantidades, eliminar líneas y vaciar el carrito.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CarritoController(ICarritoService service) : ControllerBase
{
    /// <summary>
    /// Obtiene el carrito del usuario autenticado.
    /// </summary>
    /// <returns>El carrito con sus líneas y totales.</returns>
    /// <response code="200">Devuelve el carrito del usuario.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCarritoAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.GetCarritoByUserIdAsync(userId).Match(
            onSuccess: carrito => Ok(carrito),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Agrega una línea de producto al carrito.
    /// </summary>
    /// <param name="dto">Datos del producto y cantidad a agregar.</param>
    /// <returns>El carrito actualizado.</returns>
    /// <response code="200">La línea se agregó correctamente.</response>
    /// <response code="400">Si la cantidad no es válida o hay stock insuficiente.</response>
    /// <response code="404">Si el producto no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost("lineas")]
    [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddLineaAsync([FromBody] AddLineaCarritoRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.AddLineaCarritoAsync(userId, dto.ProductoId, dto.Cantidad).Match(
            onSuccess: carrito => Ok(carrito),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                BadRequestError => BadRequest(new { message = error.Message }),
                StockInsuficienteError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Actualiza la cantidad de una línea del carrito.
    /// </summary>
    /// <param name="productoId">Identificador del producto cuya cantidad se quiere actualizar.</param>
    /// <param name="dto">Nueva cantidad deseada.</param>
    /// <returns>El carrito actualizado.</returns>
    /// <response code="200">La cantidad se actualizó correctamente.</response>
    /// <response code="400">Si la cantidad no es válida o hay stock insuficiente.</response>
    /// <response code="404">Si el carrito o el producto no existen.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPut("lineas/{productoId:long}")]
    [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLineaCantidadAsync(long productoId, [FromBody] UpdateLineaCarritoRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.UpdateLineaCantidadAsync(userId, productoId, dto.Cantidad).Match(
            onSuccess: carrito => Ok(carrito),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                BadRequestError => BadRequest(new { message = error.Message }),
                StockInsuficienteError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Elimina una línea de producto del carrito.
    /// </summary>
    /// <param name="productoId">Identificador del producto a eliminar del carrito.</param>
    /// <returns>El carrito actualizado.</returns>
    /// <response code="200">La línea se eliminó correctamente.</response>
    /// <response code="404">Si el carrito o el producto no existen en el carrito.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpDelete("lineas/{productoId:long}")]
    [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveLineaAsync(long productoId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.RemoveLineaCarritoAsync(userId, productoId).Match(
            onSuccess: carrito => Ok(carrito),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Vacía el carrito del usuario autenticado.
    /// </summary>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="204">El carrito se vació correctamente.</response>
    /// <response code="404">Si el carrito no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VaciarCarritoAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await service.VaciarCarritoAsync(userId);

        if (result.IsSuccess)
            return NoContent();

        return result.Error switch
        {
            NotFoundError => NotFound(new { message = result.Error.Message }),
            _ => StatusCode(500, new { message = result.Error.Message })
        };
    }
}
