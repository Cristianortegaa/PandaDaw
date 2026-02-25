﻿using PandaBack.Models;

namespace PandaBack.Repositories;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos del carrito de compras.
/// </summary>
public interface ICarritoRepository
{
    /// <summary>
    /// Obtiene el carrito de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Carrito encontrado o null.</returns>
    Task<Models.Carrito?> GetByUserIdAsync(string userId);
    
    /// <summary>
    /// Obtiene un carrito por su identificador.
    /// </summary>
    /// <param name="id">Identificador del carrito.</param>
    /// <returns>Carrito encontrado o null.</returns>
    Task<Models.Carrito?> GetByIdAsync(long id);
    
    /// <summary>
    /// Agrega un nuevo carrito.
    /// </summary>
    /// <param name="carrito">Carrito a agregar.</param>
    Task AddAsync(Models.Carrito carrito);
    
    /// <summary>
    /// Actualiza un carrito existente.
    /// </summary>
    /// <param name="carrito">Carrito con los datos actualizados.</param>
    Task UpdateAsync(Models.Carrito carrito);
    
    /// <summary>
    /// Elimina un carrito por su identificador.
    /// </summary>
    /// <param name="id">Identificador del carrito a eliminar.</param>
    Task DeleteAsync(long id);
}

