using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Distributed;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.Repositories;

namespace PandaBack.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repository;
    private readonly IDistributedCache _cache;

    public ProductoService(IProductoRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    private const string CacheKeyPrefix = "Producto:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    public async Task<Result<IEnumerable<Producto>, PandaError>> GetAllProductosAsync()
    {
        var productos = await _repository.GetAllAsync();
        
        return Result.Success<IEnumerable<Producto>, PandaError>(productos);
    }

    public async Task<Result<Producto, PandaError>> GetProductoByIdAsync(long id)
    {
        var cacheKey = CacheKeyPrefix + id;

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            var cachedProducto = JsonSerializer.Deserialize<Producto>(cachedData);
            if (cachedProducto != null)
            {
                return Result.Success<Producto, PandaError>(cachedProducto);
            }
        }

        var producto = await _repository.GetByIdAsync(id);
        
        if (producto == null)
            return Result.Failure<Producto, PandaError>(new NotFoundError($"Producto {id} no encontrado"));

        var serializedProducto = JsonSerializer.Serialize(producto);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration };
        await _cache.SetStringAsync(cacheKey, serializedProducto, options);

        return Result.Success<Producto, PandaError>(producto);
    }

    public async Task<Result<IEnumerable<Producto>, PandaError>> GetProductosByCategoryAsync(Categoria category)
    {
        var productos = await _repository.GetByCategoryAsync(category);
        
        return Result.Success<IEnumerable<Producto>, PandaError>(productos);
    }

    public async Task<Result<Producto, PandaError>> CreateProductoAsync(Producto producto)
    {
        if (producto.Precio < 0)
            return Result.Failure<Producto, PandaError>(new BadRequestError("El precio no puede ser negativo"));

        if (producto.Stock < 0)
            return Result.Failure<Producto, PandaError>(new BadRequestError("El stock no puede ser negativo"));

        producto.FechaAlta = DateTime.UtcNow;
        producto.IsDeleted = false;

        await _repository.AddAsync(producto);

        return Result.Success<Producto, PandaError>(producto);
    }

    public async Task<Result<Producto, PandaError>> UpdateProductoAsync(long id, Producto producto)
    {
        var productoExistente = await _repository.GetByIdAsync(id);

        if (productoExistente == null)
            return Result.Failure<Producto, PandaError>(new NotFoundError($"Producto {id} no encontrado"));

        if (producto.Precio < 0)
            return Result.Failure<Producto, PandaError>(new BadRequestError("El precio no puede ser negativo"));

        // Mapeo
        productoExistente.Nombre = producto.Nombre;
        productoExistente.Descripcion = producto.Descripcion;
        productoExistente.Precio = producto.Precio;
        productoExistente.Imagen = producto.Imagen;
        productoExistente.Stock = producto.Stock;
        productoExistente.Category = producto.Category;

        await _repository.UpdateAsync(productoExistente);
        
        await _cache.RemoveAsync(CacheKeyPrefix + id);

        return Result.Success<Producto, PandaError>(productoExistente);
    }

    public async Task<UnitResult<PandaError>> DeleteProductoAsync(long id)
    {
        var producto = await _repository.GetByIdAsync(id);

        if (producto == null)
            return UnitResult.Failure<PandaError>(new NotFoundError($"Producto {id} no encontrado"));

        await _repository.DeleteAsync(id);
        
        await _cache.RemoveAsync(CacheKeyPrefix + id);

        return UnitResult.Success<PandaError>();
    }
    
}