using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using PandaBack.Models;

namespace PandaBack.Repositories;

public class ValoracionRepository : IValoracionRepository
{
    private readonly PandaDbContext _context;
    private readonly ILogger<ValoracionRepository> _logger;

    public ValoracionRepository(PandaDbContext context, ILogger<ValoracionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Valoracion>> GetByProductoIdAsync(long productoId)
    {
        _logger.LogInformation("Obteniendo valoraciones del producto con ID: {ProductoId}", productoId);

        return await _context.Valoraciones
            .Include(v => v.User)
            .Include(v => v.Producto)
            .Where(v => v.ProductoId == productoId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Valoracion>> GetByUserIdAsync(long userId)
    {
        _logger.LogInformation("Obteniendo valoraciones del usuario con ID: {UserId}", userId);

        return await _context.Valoraciones
            .Include(v => v.User)
            .Include(v => v.Producto)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<Valoracion?> GetByIdAsync(long id)
    {
        var valoracion = await _context.Valoraciones
            .Include(v => v.User)
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (valoracion == null)
        {
            _logger.LogWarning("Valoración con ID {Id} no encontrada.", id);
        }

        return valoracion;
    }

    public async Task<Valoracion?> GetByProductoAndUserAsync(long productoId, long userId)
    {
        return await _context.Valoraciones
            .FirstOrDefaultAsync(v => v.ProductoId == productoId && v.UserId == userId);
    }

    public async Task AddAsync(Valoracion valoracion)
    {
        _logger.LogInformation("Creando valoración para producto {ProductoId} por usuario {UserId}", valoracion.ProductoId, valoracion.UserId);
        await _context.Valoraciones.AddAsync(valoracion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Valoracion valoracion)
    {
        _logger.LogInformation("Actualizando valoración con ID: {Id}", valoracion.Id);
        _context.Valoraciones.Update(valoracion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        _logger.LogInformation("Eliminando valoración con ID: {Id}", id);
        var valoracion = await GetByIdAsync(id);
        if (valoracion != null)
        {
            _context.Valoraciones.Remove(valoracion);
            await _context.SaveChangesAsync();
        }
    }
}

