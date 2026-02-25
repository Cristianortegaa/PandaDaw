﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;

namespace PandaBack.Services;

/// <summary>
/// Interfaz que define las operaciones del servicio de valoraciones.
/// </summary>
public interface IValoracionService
{
    /// <summary>
    /// Obtiene las valoraciones de un producto.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Resultado con la lista de valoraciones o error.</returns>
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByProductoAsync(long productoId);
    
    /// <summary>
    /// Obtiene las valoraciones de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado con la lista de valoraciones o error.</returns>
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByUserAsync(string userId);
    
    /// <summary>
    /// Crea una nueva valoración.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="dto">Datos de la valoración.</param>
    /// <returns>Resultado con la valoración creada o error.</returns>
    Task<Result<ValoracionResponseDto, PandaError>> CreateValoracionAsync(string userId, CreateValoracionDto dto);
    
    /// <summary>
    /// Actualiza una valoración existente.
    /// </summary>
    /// <param name="id">Identificador de la valoración.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="dto">Nuevos datos de la valoración.</param>
    /// <returns>Resultado con la valoración actualizada o error.</returns>
    Task<Result<ValoracionResponseDto, PandaError>> UpdateValoracionAsync(long id, string userId, CreateValoracionDto dto);
    
    /// <summary>
    /// Elimina una valoración.
    /// </summary>
    /// <param name="id">Identificador de la valoración.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado de la operación o error.</returns>
    Task<UnitResult<PandaError>> DeleteValoracionAsync(long id, string userId);
}

