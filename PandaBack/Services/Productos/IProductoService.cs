using CSharpFunctionalExtensions;
using PandaBack.Errors;
using PandaBack.Models;

namespace PandaBack.Services;

public interface IProductoService
{
    Task<Result<IEnumerable<Producto>, PandaError>> GetAllProductosAsync();
    Task<Result<Producto, PandaError>> GetProductoByIdAsync(long id);
    Task<Result<IEnumerable<Producto>, PandaError>> GetProductosByCategoryAsync(Categoria category);
    Task<Result<Producto, PandaError>> CreateProductoAsync(Producto producto);
    Task<Result<Producto, PandaError>> UpdateProductoAsync(long id, Producto producto);
    Task<UnitResult<PandaError>> DeleteProductoAsync(long id); 
}