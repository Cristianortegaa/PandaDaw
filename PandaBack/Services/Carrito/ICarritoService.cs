﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;

namespace PandaBack.Services;

/// <summary>
/// Interfaz que define las operaciones del servicio de carrito de compras.
/// </summary>
public interface ICarritoService
{
    /// <summary>
    /// Obtiene el carrito de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado con el carrito del usuario o error.</returns>
    Task<Result<CarritoDto, PandaError>> GetCarritoByUserIdAsync(string userId);
    
    /// <summary>
    /// Agrega una línea de producto al carrito.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="productoId">Identificador del producto.</param>
    /// <param name="cantidad">Cantidad del producto.</param>
    /// <returns>Resultado con el carrito actualizado o error.</returns>
    Task<Result<CarritoDto, PandaError>> AddLineaCarritoAsync(string userId, long productoId, int cantidad);
    
    /// <summary>
    /// Actualiza la cantidad de una línea del carrito.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="productoId">Identificador del producto.</param>
    /// <param name="cantidad">Nueva cantidad.</param>
    /// <returns>Resultado con el carrito actualizado o error.</returns>
    Task<Result<CarritoDto, PandaError>> UpdateLineaCantidadAsync(string userId, long productoId, int cantidad);
    
    /// <summary>
    /// Elimina una línea del carrito.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Resultado con el carrito actualizado o error.</returns>
    Task<Result<CarritoDto, PandaError>> RemoveLineaCarritoAsync(string userId, long productoId);
    
    /// <summary>
    /// Vacía completamente el carrito del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado de la operación o error.</returns>
    Task<UnitResult<PandaError>> VaciarCarritoAsync(string userId);
}

