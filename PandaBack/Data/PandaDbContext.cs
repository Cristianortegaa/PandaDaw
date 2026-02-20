﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PandaBack.Models;

namespace PandaBack.Data;

public class PandaDbContext : IdentityDbContext<User>
{
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    public DbSet<LineaVenta> LineasVenta => Set<LineaVenta>();
    public DbSet<LineaCarrito> LineasCarrito => Set<LineaCarrito>();
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}