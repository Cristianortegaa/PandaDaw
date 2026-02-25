﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;

namespace PandaBack.Services;

/// <summary>
/// Interfaz que define las operaciones del servicio de favoritos.
/// </summary>
public interface IFavoritoService
{
    /// <summary>
    /// Obtiene los favoritos de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado con la lista de favoritos o error.</returns>
    Task<Result<IEnumerable<FavoritoResponseDto>, PandaError>> GetUserFavoritosAsync(string userId);
    
    /// <summary>
    /// Agrega un producto a favoritos.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="dto">Datos del favorito a crear.</param>
    /// <returns>Resultado con el favorito creado o error.</returns>
    Task<Result<FavoritoResponseDto, PandaError>> AddToFavoritosAsync(string userId, CreateFavoritoDto dto);
    
    /// <summary>
    /// Elimina un producto de favoritos.
    /// </summary>
    /// <param name="id">Identificador del favorito.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado de la operación o error.</returns>
    Task<UnitResult<PandaError>> RemoveFromFavoritosAsync(long id, string userId);
}