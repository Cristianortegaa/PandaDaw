using System.Security.Claims;
using CSharpFunctionalExtensions;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;
using PandaBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para la gestión de productos favoritos.
/// </summary>
/// <remarks>
/// Este controlador permite al usuario autenticado consultar, agregar y eliminar
/// productos de su lista de favoritos.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FavoritosController(IFavoritoService service) : ControllerBase
{
    /// <summary>
    /// Obtiene la lista de favoritos del usuario autenticado.
    /// </summary>
    /// <returns>Lista de productos favoritos.</returns>
    /// <response code="200">Devuelve la lista de favoritos del usuario.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FavoritoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavoritosAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.GetUserFavoritosAsync(userId).Match(
            onSuccess: favoritos => Ok(favoritos),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Agrega un producto a la lista de favoritos.
    /// </summary>
    /// <param name="dto">Datos con el identificador del producto a agregar.</param>
    /// <returns>El favorito creado.</returns>
    /// <response code="201">El producto se agregó a favoritos correctamente.</response>
    /// <response code="400">Si el producto ya está en la lista de favoritos.</response>
    /// <response code="404">Si el producto no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FavoritoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddFavoritoAsync([FromBody] CreateFavoritoDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await service.AddToFavoritosAsync(userId, dto).Match(
            onSuccess: favorito => Created($"/api/Favoritos/{favorito.Id}", favorito),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                BadRequestError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Elimina un producto de la lista de favoritos.
    /// </summary>
    /// <param name="id">Identificador del favorito a eliminar.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="204">El favorito se eliminó correctamente.</response>
    /// <response code="404">Si el favorito no existe.</response>
    /// <response code="401">Si el usuario no está autenticado.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFavoritoAsync(long id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await service.RemoveFromFavoritosAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.Error switch
        {
            NotFoundError => NotFound(new { message = result.Error.Message }),
            _ => StatusCode(500, new { message = result.Error.Message })
        };
    }
}
