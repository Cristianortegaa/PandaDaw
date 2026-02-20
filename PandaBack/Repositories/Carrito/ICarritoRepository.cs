using PandaBack.Models;

namespace PandaBack.Repositories;

public interface ICarritoRepository
{
    Task<Models.Carrito?> GetByUserIdAsync(long userId);
    Task<Models.Carrito?> GetByIdAsync(long id);
    Task AddAsync(Models.Carrito carrito);
    Task UpdateAsync(Models.Carrito carrito);
    Task DeleteAsync(long id);
}

