using CSharpFunctionalExtensions;
using PandaBack.Dtos.Ventas;
using PandaBack.Errors;
using PandaBack.Models;

namespace PandaBack.Services;

/// <summary>
/// Interfaz que define las operaciones del servicio de ventas.
/// </summary>
public interface IVentaService
{
    /// <summary>
    /// Obtiene todas las ventas del sistema.
    /// </summary>
    /// <returns>Resultado con la lista de ventas o error.</returns>
    Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetAllVentasAsync();
    
    /// <summary>
    /// Obtiene las ventas de un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado con la lista de ventas del usuario o error.</returns>
    Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetVentasByUserAsync(string userId);
    
    /// <summary>
    /// Obtiene una venta por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <returns>Resultado con la venta encontrada o error.</returns>
    Task<Result<VentaResponseDto, PandaError>> GetVentaByIdAsync(long id);
    
    /// <summary>
    /// Crea una venta a partir del carrito de compras del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Resultado con la venta creada o error.</returns>
    Task<Result<VentaResponseDto, PandaError>> CreateVentaFromCarritoAsync(string userId);
    
    /// <summary>
    /// Actualiza el estado de una venta.
    /// </summary>
    /// <param name="id">Identificador de la venta.</param>
    /// <param name="nuevoEstado">Nuevo estado del pedido.</param>
    /// <returns>Resultado con la venta actualizada o error.</returns>
    Task<Result<VentaResponseDto, PandaError>> UpdateEstadoVentaAsync(long id, EstadoPedido nuevoEstado);
}

