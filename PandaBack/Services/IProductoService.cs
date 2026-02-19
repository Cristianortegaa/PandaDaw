using CSharpFunctionalExtensions;
using PandaBack.Models;

namespace PandaBack.Services;

public interface IProductoService
{
    Task<Result<IEnumerable<Producto>>> GetAllProductosAsync();
    Task<Result<Producto>> GetProductoByIdAsync(long id);
    Task<Result<IEnumerable<Producto>>> GetProductosByCategoryAsync(Categoria category);
    Task<Result<Producto>> CreateProductoAsync(Producto producto);
    Task<Result<Producto>> UpdateProductoAsync(long id, Producto producto);
    Task<Result> DeleteProductoAsync(long id); 
}