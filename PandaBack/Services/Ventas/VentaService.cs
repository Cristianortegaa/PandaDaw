using CSharpFunctionalExtensions;
using PandaBack.Dtos.Ventas;
using PandaBack.Errors;
using PandaBack.Mappers;
using PandaBack.Models;
using PandaBack.Repositories;

namespace PandaBack.Services;

public class VentaService : IVentaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly ICarritoRepository _carritoRepository;
    private readonly IProductoRepository _productoRepository;

    public VentaService(IVentaRepository ventaRepository, ICarritoRepository carritoRepository, IProductoRepository productoRepository)
    {
        _ventaRepository = ventaRepository;
        _carritoRepository = carritoRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetAllVentasAsync()
    {
        var ventas = await _ventaRepository.GetAllAsync();

        var response = ventas.Select(v => v.ToDto());

        return Result.Success<IEnumerable<VentaResponseDto>, PandaError>(response);
    }

    public async Task<Result<IEnumerable<VentaResponseDto>, PandaError>> GetVentasByUserAsync(long userId)
    {
        var ventas = await _ventaRepository.GetByUserIdAsync(userId);

        var response = ventas.Select(v => v.ToDto());

        return Result.Success<IEnumerable<VentaResponseDto>, PandaError>(response);
    }

    public async Task<Result<VentaResponseDto, PandaError>> GetVentaByIdAsync(long id)
    {
        var venta = await _ventaRepository.GetByIdAsync(id);

        if (venta == null)
            return Result.Failure<VentaResponseDto, PandaError>(new NotFoundError($"Venta {id} no encontrada"));

        return Result.Success<VentaResponseDto, PandaError>(venta.ToDto());
    }

    public async Task<Result<VentaResponseDto, PandaError>> CreateVentaFromCarritoAsync(long userId)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null || !carrito.LineasCarrito.Any())
            return Result.Failure<VentaResponseDto, PandaError>(new CarritoVacioError("El carrito está vacío"));

        // Verificar stock de todos los productos
        foreach (var linea in carrito.LineasCarrito)
        {
            var producto = await _productoRepository.GetByIdAsync(linea.ProductoId);

            if (producto == null)
                return Result.Failure<VentaResponseDto, PandaError>(new NotFoundError($"Producto {linea.ProductoId} no encontrado"));

            if (!producto.HasStock(linea.Cantidad))
                return Result.Failure<VentaResponseDto, PandaError>(new StockInsuficienteError($"Stock insuficiente para el producto: {producto.Nombre}"));
        }

        // Crear la venta
        var venta = new Venta
        {
            UserId = userId,
            FechaCompra = DateTime.UtcNow,
            Estado = EstadoPedido.Pendiente
        };

        // Convertir líneas del carrito en líneas de venta y reducir stock
        foreach (var lineaCarrito in carrito.LineasCarrito)
        {
            var producto = await _productoRepository.GetByIdAsync(lineaCarrito.ProductoId);

            var lineaVenta = new LineaVenta
            {
                ProductoId = lineaCarrito.ProductoId,
                Cantidad = lineaCarrito.Cantidad,
                PrecioUnitario = producto!.Precio,
                Producto = producto
            };

            venta.AddLineaVenta(lineaVenta);

            // Reducir stock del producto
            producto.ReduceStock(lineaCarrito.Cantidad);
        }

        await _ventaRepository.AddAsync(venta);

        // Vaciar el carrito después de la compra
        await _carritoRepository.DeleteAsync(carrito.Id);

        return Result.Success<VentaResponseDto, PandaError>(venta.ToDto());
    }

    public async Task<Result<VentaResponseDto, PandaError>> UpdateEstadoVentaAsync(long id, EstadoPedido nuevoEstado)
    {
        var venta = await _ventaRepository.GetByIdAsync(id);

        if (venta == null)
            return Result.Failure<VentaResponseDto, PandaError>(new NotFoundError($"Venta {id} no encontrada"));

        try
        {
            venta.UpdateEstado(nuevoEstado);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<VentaResponseDto, PandaError>(new OperacionNoPermitidaError(ex.Message));
        }

        await _ventaRepository.UpdateAsync(venta);

        return Result.Success<VentaResponseDto, PandaError>(venta.ToDto());
    }
}

