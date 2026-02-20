using CSharpFunctionalExtensions;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;
using PandaBack.Mappers;
using PandaBack.Models;
using PandaBack.Repositories;

namespace PandaBack.Services;

public class CarritoService : ICarritoService
{
    private readonly ICarritoRepository _carritoRepository;
    private readonly IProductoRepository _productoRepository;

    public CarritoService(ICarritoRepository carritoRepository, IProductoRepository productoRepository)
    {
        _carritoRepository = carritoRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Result<CarritoDto, PandaError>> GetCarritoByUserIdAsync(long userId)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null)
        {
            // Si no existe, creamos uno vacío para el usuario
            carrito = new Models.Carrito
            {
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            };
            await _carritoRepository.AddAsync(carrito);
        }

        return Result.Success<CarritoDto, PandaError>(carrito.ToDto());
    }

    public async Task<Result<CarritoDto, PandaError>> AddLineaCarritoAsync(long userId, long productoId, int cantidad)
    {
        if (cantidad <= 0)
            return Result.Failure<CarritoDto, PandaError>(new BadRequestError("La cantidad debe ser mayor a 0"));

        var producto = await _productoRepository.GetByIdAsync(productoId);

        if (producto == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError($"Producto {productoId} no encontrado"));

        if (!producto.HasStock(cantidad))
            return Result.Failure<CarritoDto, PandaError>(new StockInsuficienteError($"Stock insuficiente para el producto: {producto.Nombre}"));

        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null)
        {
            carrito = new Models.Carrito
            {
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            };
            await _carritoRepository.AddAsync(carrito);
        }

        var lineaExistente = carrito.LineasCarrito.FirstOrDefault(l => l.ProductoId == productoId);

        if (lineaExistente != null)
        {
            var cantidadTotal = lineaExistente.Cantidad + cantidad;
            if (!producto.HasStock(cantidadTotal))
                return Result.Failure<CarritoDto, PandaError>(new StockInsuficienteError($"Stock insuficiente para el producto: {producto.Nombre}"));

            lineaExistente.IncrementQuantity(cantidad);
        }
        else
        {
            var nuevaLinea = new LineaCarrito
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                Producto = producto
            };
            carrito.LineasCarrito.Add(nuevaLinea);
        }

        carrito.UpdatedAt = DateTime.UtcNow;
        await _carritoRepository.UpdateAsync(carrito);

        return Result.Success<CarritoDto, PandaError>(carrito.ToDto());
    }

    public async Task<Result<CarritoDto, PandaError>> UpdateLineaCantidadAsync(long userId, long productoId, int cantidad)
    {
        if (cantidad <= 0)
            return Result.Failure<CarritoDto, PandaError>(new BadRequestError("La cantidad debe ser mayor a 0"));

        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError("Carrito no encontrado"));

        var linea = carrito.LineasCarrito.FirstOrDefault(l => l.ProductoId == productoId);

        if (linea == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError($"Producto {productoId} no encontrado en el carrito"));

        var producto = await _productoRepository.GetByIdAsync(productoId);

        if (producto == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError($"Producto {productoId} no encontrado"));

        if (!producto.HasStock(cantidad))
            return Result.Failure<CarritoDto, PandaError>(new StockInsuficienteError($"Stock insuficiente para el producto: {producto.Nombre}"));

        linea.Cantidad = cantidad;
        carrito.UpdatedAt = DateTime.UtcNow;

        await _carritoRepository.UpdateAsync(carrito);

        return Result.Success<CarritoDto, PandaError>(carrito.ToDto());
    }

    public async Task<Result<CarritoDto, PandaError>> RemoveLineaCarritoAsync(long userId, long productoId)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError("Carrito no encontrado"));

        var linea = carrito.LineasCarrito.FirstOrDefault(l => l.ProductoId == productoId);

        if (linea == null)
            return Result.Failure<CarritoDto, PandaError>(new NotFoundError($"Producto {productoId} no encontrado en el carrito"));

        carrito.RemoveLineaCarrito(linea);

        await _carritoRepository.UpdateAsync(carrito);

        return Result.Success<CarritoDto, PandaError>(carrito.ToDto());
    }

    public async Task<UnitResult<PandaError>> VaciarCarritoAsync(long userId)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);

        if (carrito == null)
            return UnitResult.Failure<PandaError>(new NotFoundError("Carrito no encontrado"));

        await _carritoRepository.DeleteAsync(carrito.Id);

        return UnitResult.Success<PandaError>();
    }
}

