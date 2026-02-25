﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

/// <summary>
/// Representa un producto favorito de un usuario.
/// </summary>
[Table("favoritos")]
[Index(nameof(UserId), nameof(ProductoId), IsUnique = true)]
public class Favorito
{
    /// <summary>
    /// Identificador único del favorito.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// Fecha en que se agregó el producto a favoritos.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identificador del usuario.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Usuario propietario del favorito.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Identificador del producto favorito.
    /// </summary>
    public long ProductoId { get; set; }

    /// <summary>
    /// Producto favorito.
    /// </summary>
    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}