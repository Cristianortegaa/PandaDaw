using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using PandaBack.Models;

namespace PandaBack.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly PandaDbContext _context;
    private readonly ILogger<ProductoRepository> _logger;

    public ProductoRepository(PandaDbContext context, ILogger<ProductoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los productos activos.");
        
        return await _context.Productos
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(long id)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (producto == null)
        {
            _logger.LogWarning("Producto con ID {Id} no encontrado o eliminado.", id);
        }

        return producto;
    }

    public async Task<IEnumerable<Producto>> GetByCategoryAsync(Categoria category)
    {
        _logger.LogInformation("Buscando productos en categoría: {Category}", category);

        return await _context.Productos
            .Where(p => p.Category == category && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task AddAsync(Producto producto)
    {
        _logger.LogInformation("Creando producto con ID: {producto}", producto.Id);
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Producto producto)
    {
        _logger.LogInformation("Modificando producto con ID: {producto}", producto.Id);
        producto.IsDeleted = false;
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        _logger.LogInformation("Eliminando producto con ID: {producto}", id);
        var producto = await GetByIdAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
    }
}