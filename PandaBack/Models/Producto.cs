using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

[Table("productos")]
[Index(nameof(Category))]
[Index(nameof(IsDeleted))]
public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        
        [Column(TypeName = "text")] 
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public string? Imagen { get; set; }

        [Required]
        public int Stock { get; set; }
        
        [Required]
        [Column(TypeName = "varchar(50)")]
        public Categoria Category { get; set; }
        
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
        
        public bool IsDeleted { get; set; } = false;
        
        public bool HasStock(int quantity)
        {
            return !IsDeleted && Stock >= quantity;
        }

        public void ReduceStock(int quantity)
        {
            if (!HasStock(quantity))
            {
                throw new InvalidOperationException($"Stock insuficiente para el producto: {Nombre}");
            }
            Stock -= quantity;
        }

        public void IncreaseStock(int quantity)
        {
            Stock += quantity;
        }
    }