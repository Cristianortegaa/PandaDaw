using PandaBack.Models;

namespace PandaBack.Repositories;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de productos.
/// </summary>
public interface IProductoRepository
{
    /// <summary>
    /// Obtiene todos los productos activos.
    /// </summary>
    /// <returns>Lista de productos.</returns>
    Task<IEnumerable<Producto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>Producto encontrado o null.</returns>
    Task<Producto?> GetByIdAsync(long id);
    
    /// <summary>
    /// Obtiene productos filtrados por categoría.
    /// </summary>
    /// <param name="category">Categoría de productos.</param>
    /// <returns>Lista de productos de la categoría.</returns>
    Task<IEnumerable<Producto>> GetByCategoryAsync(Categoria category);
    
    /// <summary>
    /// Agrega un nuevo producto.
    /// </summary>
    /// <param name="producto">Producto a agregar.</param>
    Task AddAsync(Producto producto);
    
    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="producto">Producto con los datos actualizados.</param>
    Task UpdateAsync(Producto producto); 
    
    /// <summary>
    /// Elimina un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    Task DeleteAsync(long id);
}