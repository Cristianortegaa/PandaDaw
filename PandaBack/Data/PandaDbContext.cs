using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PandaBack.Models;

namespace PandaBack.Data;

/// <summary>
/// Contexto de base de datos de Entity Framework para la aplicación PandaDaw.
/// </summary>
public class PandaDbContext : IdentityDbContext<User>
{
    public PandaDbContext(DbContextOptions<PandaDbContext> options) : base(options) { }

    /// <summary>
    /// Conjunto de ventas.
    /// </summary>
    public DbSet<Venta> Ventas => Set<Venta>();
    
    /// <summary>
    /// Conjunto de carritos.
    /// </summary>
    public DbSet<Carrito> Carritos => Set<Carrito>();
    
    /// <summary>
    /// Conjunto de productos.
    /// </summary>
    public DbSet<Producto> Productos => Set<Producto>();
    
    /// <summary>
    /// Conjunto de favoritos.
    /// </summary>
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    
    /// <summary>
    /// Conjunto de valoraciones.
    /// </summary>
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    
    /// <summary>
    /// Conjunto de líneas de venta.
    /// </summary>
    public DbSet<LineaVenta> LineasVenta => Set<LineaVenta>();
    
    /// <summary>
    /// Conjunto de líneas de carrito.
    /// </summary>
    public DbSet<LineaCarrito> LineasCarrito => Set<LineaCarrito>();
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}