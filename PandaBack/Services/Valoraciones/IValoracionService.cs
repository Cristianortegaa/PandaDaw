using CSharpFunctionalExtensions;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface IValoracionService
{
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByProductoAsync(long productoId);
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByUserAsync(long userId);
    Task<Result<ValoracionResponseDto, PandaError>> CreateValoracionAsync(long userId, CreateValoracionDto dto);
    Task<Result<ValoracionResponseDto, PandaError>> UpdateValoracionAsync(long id, long userId, CreateValoracionDto dto);
    Task<UnitResult<PandaError>> DeleteValoracionAsync(long id, long userId);
}

