using PandaBack.Models;

namespace PandaBack.Repository;

public interface IFavoritoRepository
{
    Task<IEnumerable<Favorito>> GetByUserIdAsync(long userId); 
    Task<Favorito?> GetByIdAsync(long id);
    
    Task<Favorito?> GetByProductAndUserAsync(long productoId, long userId); 
    Task AddAsync(Favorito favorito);
    Task DeleteAsync(long id);
}