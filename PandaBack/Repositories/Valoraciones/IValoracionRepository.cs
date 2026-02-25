﻿using PandaBack.Models;

namespace PandaBack.Repositories;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de valoraciones.
/// </summary>
public interface IValoracionRepository
{
    /// <summary>
    /// Obtiene las valoraciones de un producto.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Lista de valoraciones del producto.</returns>
    Task<IEnumerable<Valoracion>> GetByProductoIdAsync(long productoId);
    
    /// <summary>
    /// Obtiene las valoraciones de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Lista de valoraciones del usuario.</returns>
    Task<IEnumerable<Valoracion>> GetByUserIdAsync(string userId);
    
    /// <summary>
    /// Obtiene una valoración por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la valoración.</param>
    /// <returns>Valoración encontrada o null.</returns>
    Task<Valoracion?> GetByIdAsync(long id);
    
    /// <summary>
    /// Obtiene una valoración por producto y usuario.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Valoración encontrada o null.</returns>
    Task<Valoracion?> GetByProductoAndUserAsync(long productoId, string userId);
    
    /// <summary>
    /// Agrega una nueva valoración.
    /// </summary>
    /// <param name="valoracion">Valoración a agregar.</param>
    Task AddAsync(Valoracion valoracion);
    
    /// <summary>
    /// Actualiza una valoración existente.
    /// </summary>
    /// <param name="valoracion">Valoración con los datos actualizados.</param>
    Task UpdateAsync(Valoracion valoracion);
    
    /// <summary>
    /// Elimina una valoración por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la valoración a eliminar.</param>
    Task DeleteAsync(long id);
}

