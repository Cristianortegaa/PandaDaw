﻿using PandaBack.Models;

namespace PandaBack.Repository;

public interface IFavoritoRepository
{
    Task<IEnumerable<Favorito>> GetByUserIdAsync(string userId); 
    Task<Favorito?> GetByIdAsync(long id);
    
    Task<Favorito?> GetByProductAndUserAsync(long productoId, string userId); 
    Task AddAsync(Favorito favorito);
    Task DeleteAsync(long id);
}