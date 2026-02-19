using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Distributed;
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

    public async Task<Result<IEnumerable<Producto>>> GetAllProductosAsync()
    {
        var productos = await _repository.GetAllAsync();
        
        return Result.Success(productos);
    }

    public async Task<Result<Producto>> GetProductoByIdAsync(long id)
    {
        var cacheKey = CacheKeyPrefix + id;

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            var cachedProducto = JsonSerializer.Deserialize<Producto>(cachedData);
            if (cachedProducto != null)
            {
                return Result.Success(cachedProducto);
            }
        }

        var producto = await _repository.GetByIdAsync(id);
        if (producto == null)
            return Result.Failure<Producto>($"Producto {id} no encontrado");

        var serializedProducto = JsonSerializer.Serialize(producto);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration };
        await _cache.SetStringAsync(cacheKey, serializedProducto, options);

        return Result.Success(producto);
    }

    public async Task<Result<IEnumerable<Producto>>> GetProductosByCategoryAsync(Categoria category)
    {
        var productos = await _repository.GetByCategoryAsync(category);
        
        return Result.Success(productos);
    }

    public async Task<Result<Producto>> CreateProductoAsync(Producto producto)
    {
        if (producto.Precio < 0)
            return Result.Failure<Producto>("El precio no puede ser negativo");

        if (producto.Stock < 0)
            return Result.Failure<Producto>("El stock no puede ser negativo");

        producto.FechaAlta = DateTime.UtcNow;
        producto.IsDeleted = false;

        await _repository.AddAsync(producto);

        return Result.Success(producto);
    }

    public async Task<Result<Producto>> UpdateProductoAsync(long id, Producto producto)
    {
        var productoExistente = await _repository.GetByIdAsync(id);

        if (productoExistente == null)
            return Result.Failure<Producto>($"Producto {id} no encontrado");

        if (producto.Precio < 0)
            return Result.Failure<Producto>("El precio no puede ser negativo");

        productoExistente.Nombre = producto.Nombre;
        productoExistente.Descripcion = producto.Descripcion;
        productoExistente.Precio = producto.Precio;
        productoExistente.Imagen = producto.Imagen;
        productoExistente.Stock = producto.Stock;
        productoExistente.Category = producto.Category;

        await _repository.UpdateAsync(productoExistente);
        
        await _cache.RemoveAsync(CacheKeyPrefix + id);

        return Result.Success(productoExistente);
    }

    public async Task<Result> DeleteProductoAsync(long id)
    {
        var producto = await _repository.GetByIdAsync(id);

        if (producto == null)
            return Result.Failure($"Producto {id} no encontrado");

        await _repository.DeleteAsync(id);
        
        await _cache.RemoveAsync(CacheKeyPrefix + id);

        return Result.Success();
    }
}