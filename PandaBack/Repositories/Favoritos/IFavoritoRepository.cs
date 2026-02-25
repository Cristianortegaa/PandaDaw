﻿using PandaBack.Models;

namespace PandaBack.Repository;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de favoritos.
/// </summary>
public interface IFavoritoRepository
{
    /// <summary>
    /// Obtiene los favoritos de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Lista de favoritos del usuario.</returns>
    Task<IEnumerable<Favorito>> GetByUserIdAsync(string userId); 
    
    /// <summary>
    /// Obtiene un favorito por su identificador.
    /// </summary>
    /// <param name="id">Identificador del favorito.</param>
    /// <returns>Favorito encontrado o null.</returns>
    Task<Favorito?> GetByIdAsync(long id);
    
    /// <summary>
    /// Obtiene un favorito por producto y usuario.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Favorito encontrado o null.</returns>
    Task<Favorito?> GetByProductAndUserAsync(long productoId, string userId); 
    
    /// <summary>
    /// Agrega un nuevo favorito.
    /// </summary>
    /// <param name="favorito">Favorito a agregar.</param>
    Task AddAsync(Favorito favorito);
    
    /// <summary>
    /// Elimina un favorito por su identificador.
    /// </summary>
    /// <param name="id">Identificador del favorito a eliminar.</param>
    Task DeleteAsync(long id);
}