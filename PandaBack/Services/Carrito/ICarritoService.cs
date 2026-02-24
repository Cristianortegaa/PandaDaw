﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface ICarritoService
{
    Task<Result<CarritoDto, PandaError>> GetCarritoByUserIdAsync(string userId);
    Task<Result<CarritoDto, PandaError>> AddLineaCarritoAsync(string userId, long productoId, int cantidad);
    Task<Result<CarritoDto, PandaError>> UpdateLineaCantidadAsync(string userId, long productoId, int cantidad);
    Task<Result<CarritoDto, PandaError>> RemoveLineaCarritoAsync(string userId, long productoId);
    Task<UnitResult<PandaError>> VaciarCarritoAsync(string userId);
}

