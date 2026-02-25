namespace PandaBack.Dtos.Valoraciones;

/// <summary>
/// DTO de respuesta de valoración.
/// </summary>
public class ValoracionResponseDto
{
    /// <summary>
    /// Identificador único de la valoración.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Número de estrellas otorgadas.
    /// </summary>
    public int Estrellas { get; set; }
    
    /// <summary>
    /// Texto de la reseña.
    /// </summary>
    public string Resena { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de creación de la valoración.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Identificador del usuario que realizó la valoración.
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del usuario que realizó la valoración.
    /// </summary>
    public string UsuarioNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Avatar del usuario que realizó la valoración.
    /// </summary>
    public string UsuarioAvatar { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificador del producto valorado.
    /// </summary>
    public long ProductoId { get; set; }
    
    /// <summary>
    /// Nombre del producto valorado.
    /// </summary>
    public string ProductoNombre { get; set; } = string.Empty;
}
