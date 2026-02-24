﻿using CSharpFunctionalExtensions;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;

namespace PandaBack.Services;

public interface IValoracionService
{
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByProductoAsync(long productoId);
    Task<Result<IEnumerable<ValoracionResponseDto>, PandaError>> GetValoracionesByUserAsync(string userId);
    Task<Result<ValoracionResponseDto, PandaError>> CreateValoracionAsync(string userId, CreateValoracionDto dto);
    Task<Result<ValoracionResponseDto, PandaError>> UpdateValoracionAsync(long id, string userId, CreateValoracionDto dto);
    Task<UnitResult<PandaError>> DeleteValoracionAsync(long id, string userId);
}

