using PandaBack.Models;

namespace PandaBack.Repositories;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<Producto?> GetByIdAsync(long id);
    Task<IEnumerable<Producto>> GetByCategoryAsync(Categoria category);
    Task AddAsync(Producto producto);
    
    Task UpdateAsync(Producto producto); 
    Task DeleteAsync(long id);
}