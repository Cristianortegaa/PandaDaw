using CSharpFunctionalExtensions;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface IFavoritoService
{
    Task<Result<IEnumerable<FavoritoResponseDto>, PandaError>> GetUserFavoritosAsync(long userId);
    
    Task<Result<FavoritoResponseDto, PandaError>> AddToFavoritosAsync(long userId, CreateFavoritoDto dto);
    
    Task<UnitResult<PandaError>> RemoveFromFavoritosAsync(long id, long userId);
}