﻿using PandaBack.Models;

namespace PandaBack.Repositories;

public interface IValoracionRepository
{
    Task<IEnumerable<Valoracion>> GetByProductoIdAsync(long productoId);
    Task<IEnumerable<Valoracion>> GetByUserIdAsync(string userId);
    Task<Valoracion?> GetByIdAsync(long id);
    Task<Valoracion?> GetByProductoAndUserAsync(long productoId, string userId);
    Task AddAsync(Valoracion valoracion);
    Task UpdateAsync(Valoracion valoracion);
    Task DeleteAsync(long id);
}

