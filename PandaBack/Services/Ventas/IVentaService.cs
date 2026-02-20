using CSharpFunctionalExtensions;
using PandaBack.Dtos.Ventas;
using PandaBack.Errors;
using PandaBack.Models;

namespace PandaBack.Services;

public interface IVentaService
{
    Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetAllVentasAsync();
    Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetVentasByUserAsync(long userId);
    Task<Result<VentaResponseDto, PandaError>> GetVentaByIdAsync(long id);
    Task<Result<VentaResponseDto, PandaError>> CreateVentaFromCarritoAsync(long userId);
    Task<Result<VentaResponseDto, PandaError>> UpdateEstadoVentaAsync(long id, EstadoPedido nuevoEstado);
}

