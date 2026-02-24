﻿using PandaBack.Models;

namespace PandaBack.Repositories;

public interface IVentaRepository
{
    Task<IEnumerable<Venta>> GetAllAsync();
    Task<IEnumerable<Venta>> GetByUserIdAsync(string userId);
    Task<Venta?> GetByIdAsync(long id);
    Task AddAsync(Venta venta);
    Task UpdateAsync(Venta venta);
}

