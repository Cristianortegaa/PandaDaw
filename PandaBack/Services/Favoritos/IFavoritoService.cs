﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface IFavoritoService
{
    Task<Result<IEnumerable<FavoritoResponseDto>, PandaError>> GetUserFavoritosAsync(string userId);
    
    Task<Result<FavoritoResponseDto, PandaError>> AddToFavoritosAsync(string userId, CreateFavoritoDto dto);
    
    Task<UnitResult<PandaError>> RemoveFromFavoritosAsync(long id, string userId);
}