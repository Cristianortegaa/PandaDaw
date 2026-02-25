using CSharpFunctionalExtensions;
using PandaBack.Dtos.Productos;
using PandaBack.Errors;
using PandaBack.Mappers;
using PandaBack.Models;
using PandaBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para la gestión de productos.
/// </summary>
/// <remarks>
/// Este controlador maneja todas las operaciones CRUD para los productos,
/// incluyendo consulta por categoría, creación, actualización y eliminación lógica.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController(IProductoService service) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los productos disponibles.
    /// </summary>
    /// <returns>Lista de productos.</returns>
    /// <response code="200">Devuelve la lista completa de productos.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync()
    {
        return await service.GetAllProductosAsync().Match(
            onSuccess: productos => Ok(productos.Select(p => p.ToDto())),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Obtiene un producto específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>El producto encontrado.</returns>
    /// <response code="200">Devuelve el producto encontrado.</response>
    /// <response code="404">Si no se encuentra el producto.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(long id)
    {
        return await service.GetProductoByIdAsync(id).Match(
            onSuccess: producto => Ok(producto.ToDto()),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Obtiene productos filtrados por categoría.
    /// </summary>
    /// <param name="categoria">Nombre de la categoría (Audio, Imagen, Smartphones, Laptops, Gaming).</param>
    /// <returns>Lista de productos de la categoría indicada.</returns>
    /// <response code="200">Devuelve la lista de productos de la categoría.</response>
    /// <response code="400">Si la categoría no es válida.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpGet("categoria/{categoria}")]
    [ProducesResponseType(typeof(IEnumerable<ProductoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCategoryAsync(string categoria)
    {
        if (!Enum.TryParse<Categoria>(categoria, true, out var cat))
            return BadRequest(new { message = $"Categoría '{categoria}' no válida" });

        return await service.GetProductosByCategoryAsync(cat).Match(
            onSuccess: productos => Ok(productos.Select(p => p.ToDto())),
            onFailure: error => StatusCode(500, new { message = error.Message })
        );
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="dto">Datos del producto a crear.</param>
    /// <returns>El producto creado.</returns>
    /// <response code="201">El producto se creó correctamente.</response>
    /// <response code="400">Si los datos de entrada son inválidos.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromBody] ProductoRequestDto dto)
    {
        var producto = dto.ToModel();
        return await service.CreateProductoAsync(producto).Match(
            onSuccess: created => Created($"/api/Productos/{created.Id}", created.ToDto()),
            onFailure: error => error switch
            {
                BadRequestError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="dto">Datos actualizados del producto.</param>
    /// <returns>El producto actualizado.</returns>
    /// <response code="200">El producto se actualizó correctamente.</response>
    /// <response code="400">Si los datos de entrada son inválidos.</response>
    /// <response code="404">Si no se encuentra el producto.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] ProductoRequestDto dto)
    {
        var producto = dto.ToModel();
        return await service.UpdateProductoAsync(id, producto).Match(
            onSuccess: updated => Ok(updated.ToDto()),
            onFailure: error => error switch
            {
                NotFoundError => NotFound(new { message = error.Message }),
                BadRequestError => BadRequest(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            }
        );
    }

    /// <summary>
    /// Elimina un producto (eliminación lógica).
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="204">El producto se eliminó correctamente.</response>
    /// <response code="404">Si no se encuentra el producto.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        var result = await service.DeleteProductoAsync(id);

        if (result.IsSuccess)
            return NoContent();

        return result.Error switch
        {
            NotFoundError => NotFound(new { message = result.Error.Message }),
            _ => StatusCode(500, new { message = result.Error.Message })
        };
    }
}
