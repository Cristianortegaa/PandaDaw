using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using PandaBack.Models;

namespace PandaBack.Repositories;

public class CarritoRepository : ICarritoRepository
{
    private readonly PandaDbContext _context;
    private readonly ILogger<CarritoRepository> _logger;

    public CarritoRepository(PandaDbContext context, ILogger<CarritoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Models.Carrito?> GetByUserIdAsync(long userId)
    {
        _logger.LogInformation("Obteniendo carrito del usuario con ID: {UserId}", userId);

        return await _context.Carritos
            .Include(c => c.LineasCarrito)
            .ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Models.Carrito?> GetByIdAsync(long id)
    {
        var carrito = await _context.Carritos
            .Include(c => c.LineasCarrito)
            .ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (carrito == null)
        {
            _logger.LogWarning("Carrito con ID {Id} no encontrado.", id);
        }

        return carrito;
    }

    public async Task AddAsync(Models.Carrito carrito)
    {
        _logger.LogInformation("Creando carrito para usuario con ID: {UserId}", carrito.UserId);
        await _context.Carritos.AddAsync(carrito);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Models.Carrito carrito)
    {
        _logger.LogInformation("Actualizando carrito con ID: {Id}", carrito.Id);
        _context.Carritos.Update(carrito);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        _logger.LogInformation("Eliminando carrito con ID: {Id}", id);
        var carrito = await GetByIdAsync(id);
        if (carrito != null)
        {
            _context.Carritos.Remove(carrito);
            await _context.SaveChangesAsync();
        }
    }
}

