using CSharpFunctionalExtensions;
using PandaBack.Errors;
using PandaBack.Models;

namespace PandaBack.Services;

/// <summary>
/// Interfaz que define las operaciones del servicio de productos.
/// </summary>
public interface IProductoService
{
    /// <summary>
    /// Obtiene todos los productos activos.
    /// </summary>
    /// <returns>Resultado con la lista de productos o error.</returns>
    Task<Result<IEnumerable<Producto>, PandaError>> GetAllProductosAsync();
    
    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>Resultado con el producto encontrado o error.</returns>
    Task<Result<Producto, PandaError>> GetProductoByIdAsync(long id);
    
    /// <summary>
    /// Obtiene productos filtrados por categoría.
    /// </summary>
    /// <param name="category">Categoría de productos.</param>
    /// <returns>Resultado con la lista de productos de la categoría o error.</returns>
    Task<Result<IEnumerable<Producto>, PandaError>> GetProductosByCategoryAsync(Categoria category);
    
    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="producto">Datos del producto a crear.</param>
    /// <returns>Resultado con el producto creado o error.</returns>
    Task<Result<Producto, PandaError>> CreateProductoAsync(Producto producto);
    
    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="producto">Nuevos datos del producto.</param>
    /// <returns>Resultado con el producto actualizado o error.</returns>
    Task<Result<Producto, PandaError>> UpdateProductoAsync(long id, Producto producto);
    
    /// <summary>
    /// Elimina un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <returns>Resultado de la operación o error.</returns>
    Task<UnitResult<PandaError>> DeleteProductoAsync(long id); 
}