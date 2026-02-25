﻿using PandaBack.Models;

namespace PandaBack.Repositories;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de ventas.
/// </summary>
public interface IVentaRepository
{
    /// <summary>
    /// Obtiene todas las ventas.
    /// </summary>
    /// <returns>Lista de ventas.</returns>
    Task<IEnumerable<Venta>> GetAllAsync();
    
    /// <summary>
    /// Obtiene las ventas de un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Lista de ventas del usuario.</returns>
    Task<IEnumerable<Venta>> GetByUserIdAsync(string userId);
    
    /// <summary>
    /// Obtiene una venta por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <returns>Venta encontrada o null.</returns>
    Task<Venta?> GetByIdAsync(long id);
    
    /// <summary>
    /// Agrega una nueva venta.
    /// </summary>
    /// <param name="venta">Venta a agregar.</param>
    Task AddAsync(Venta venta);
    
    /// <summary>
    /// Actualiza una venta existente.
    /// </summary>
    /// <param name="venta">Venta con los datos actualizados.</param>
    Task UpdateAsync(Venta venta);
}

