using System.Security.Claims;
using CSharpFunctionalExtensions;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;
using PandaBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para la gestión de valoraciones de productos.
/// </summary>
/// <remarks>
/// Este controlador permite consultar, crear, actualizar y eliminar valoraciones (reseñas)
/// de productos. Las operaciones de escritura requieren autenticación.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ValoracionesController(IValoracionService service) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las valoraciones de un producto.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Lista de valoraciones del producto.</returns>
    /// <response code="200">Devuelve las valoraciones del producto.</response>
    /// <response code="404">Si el producto no existe.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("producto/{productoId:long}")]
    [ProducesResponseType(typeof(IEnumerable<ValoracionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProductoAsync(long productoId)
    {
        return await service.GetValoracionesByProductoAsync(productoId).Match(
            onSuccess: valoraciones => Ok(valoraciones),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Obtiene todas las valoraciones del usuario autenticado.
    /// </summary>
    /// <returns>Lista de valoraciones del usuario.</returns>
    /// <response code="200">Devuelve las valoraciones del usuario.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("mis-valoraciones")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ValoracionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUserAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.GetValoracionesByUserAsync(userId).Match(
            onSuccess: valoraciones => Ok(valoraciones),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Crea una nueva valoración para un producto.
    /// </summary>
    /// <param name="dto">Datos de la valoración (producto, estrellas, reseña).</param>
    /// <returns>La valoración creada.</returns>
    /// <response code="201">La valoración se creó correctamente.</response>
    /// <response code="400">Si los datos son inválidos.</response>
    /// <response code="404">Si el producto no existe.</response>
    /// <response code="409">Si el usuario ya valoró este producto.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ValoracionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateValoracionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.CreateValoracionAsync(userId, dto).Match(
            onSuccess: valoracion => Created($"/api/Valoraciones/{valoracion.Id}", valoracion),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                ConflictError => Conflict(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Actualiza una valoración existente del usuario autenticado.
    /// </summary>
    /// <param name="id">Identificador de la valoración a actualizar.</param>
    /// <param name="dto">Datos actualizados de la valoración.</param>
    /// <returns>La valoración actualizada.</returns>
    /// <response code="200">La valoración se actualizó correctamente.</response>
    /// <response code="403">Si la valoración pertenece a otro usuario.</response>
    /// <response code="404">Si la valoración no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPut("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(ValoracionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] CreateValoracionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.UpdateValoracionAsync(id, userId, dto).Match(
            onSuccess: valoracion => Ok(valoracion),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                OperacionNoPermitidaError => StatusCode(403, new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Elimina una valoración del usuario autenticado.
    /// </summary>
    /// <param name="id">Identificador de la valoración a eliminar.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="204">La valoración se eliminó correctamente.</response>
    /// <response code="403">Si la valoración pertenece a otro usuario.</response>
    /// <response code="404">Si la valoración no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await service.DeleteValoracionAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.Error switch
        {
            NotFoundError => NotFound(new { message = result.Error.Message }),
            OperacionNoPermitidaError => StatusCode(403, new { message = result.Error.Message }),
            _ => StatusCode(500, new { message = result.Error.Message })
        };
    }
}
