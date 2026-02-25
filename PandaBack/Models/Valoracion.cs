﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

/// <summary>
/// Representa una valoración (reseña) de un producto realizada por un usuario.
/// </summary>
[Table("valoraciones")]
[Index(nameof(ProductoId))]
[Index(nameof(UserId), nameof(ProductoId), IsUnique = true)]
public class Valoracion
{
    /// <summary>
    /// Identificador único de la valoración.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Número de estrellas otorgadas (1-5).
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5 estrellas.")]
    public int Estrellas { get; set; }

    /// <summary>
    /// Texto de la reseña.
    /// </summary>
    [Required]
    [MaxLength(500, ErrorMessage = "La reseña no puede superar los 500 caracteres.")]
    [Column(TypeName = "text")]
    public string Resena { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación de la valoración.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Identificador del usuario que realizó la valoración.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la valoración.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Identificador del producto valorado.
    /// </summary>
    public long ProductoId { get; set; }

    /// <summary>
    /// Producto valorado.
    /// </summary>
    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}