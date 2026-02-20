using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using PandaBack.Models;

namespace PandaBack.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly PandaDbContext _context;
    private readonly ILogger<VentaRepository> _logger;

    public VentaRepository(PandaDbContext context, ILogger<VentaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Venta>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todas las ventas.");

        return await _context.Ventas
            .Include(v => v.User)
            .Include(v => v.Lineas)
            .ThenInclude(l => l.Producto)
            .OrderByDescending(v => v.FechaCompra)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetByUserIdAsync(long userId)
    {
        _logger.LogInformation("Obteniendo ventas del usuario con ID: {UserId}", userId);

        return await _context.Ventas
            .Include(v => v.User)
            .Include(v => v.Lineas)
            .ThenInclude(l => l.Producto)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.FechaCompra)
            .ToListAsync();
    }

    public async Task<Venta?> GetByIdAsync(long id)
    {
        var venta = await _context.Ventas
            .Include(v => v.User)
            .Include(v => v.Lineas)
            .ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
        {
            _logger.LogWarning("Venta con ID {Id} no encontrada.", id);
        }

        return venta;
    }

    public async Task AddAsync(Venta venta)
    {
        _logger.LogInformation("Creando venta para usuario con ID: {UserId}", venta.UserId);
        await _context.Ventas.AddAsync(venta);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Venta venta)
    {
        _logger.LogInformation("Actualizando venta con ID: {Id}", venta.Id);
        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
    }
}

