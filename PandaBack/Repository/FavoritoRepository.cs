using Microsoft.EntityFrameworkCore;
using PandaBack.Data; // Ajusta según el nombre de tu DbContext
using PandaBack.Models;

namespace PandaBack.Repository;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly PandaDbContext _context;

    public FavoritoRepository(PandaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Favorito>> GetByUserIdAsync(long userId)
    {
        return await _context.Favoritos
            .Include(f => f.Producto) 
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public async Task<Favorito?> GetByIdAsync(long id)
    {
        return await _context.Favoritos.FindAsync(id);
    }

    public async Task<Favorito?> GetByProductAndUserAsync(long productoId, long userId)
    {
        return await _context.Favoritos
            .FirstOrDefaultAsync(f => f.ProductoId == productoId && f.UserId == userId);
    }

    public async Task AddAsync(Favorito favorito)
    {
        await _context.Favoritos.AddAsync(favorito);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var favorito = await GetByIdAsync(id);
        if (favorito != null)
        {
            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();
        }
    }
}