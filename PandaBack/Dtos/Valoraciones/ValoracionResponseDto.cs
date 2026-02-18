namespace PandaBack.Dtos.Valoraciones;

public class ValoracionResponseDto
{
    public long Id { get; set; }
    public int Estrellas { get; set; }
    public string Resena { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    // Datos del Autor (Usuario)
    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string UsuarioAvatar { get; set; } = string.Empty;
    
    // Opcional: ID del producto por si listamos reseñas de un usuario
    public long ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
}
