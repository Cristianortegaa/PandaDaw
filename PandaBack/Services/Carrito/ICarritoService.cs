using CSharpFunctionalExtensions;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface ICarritoService
{
    Task<Result<CarritoDto, PandaError>> GetCarritoByUserIdAsync(long userId);
    Task<Result<CarritoDto, PandaError>> AddLineaCarritoAsync(long userId, long productoId, int cantidad);
    Task<Result<CarritoDto, PandaError>> UpdateLineaCantidadAsync(long userId, long productoId, int cantidad);
    Task<Result<CarritoDto, PandaError>> RemoveLineaCarritoAsync(long userId, long productoId);
    Task<UnitResult<PandaError>> VaciarCarritoAsync(long userId);
}

