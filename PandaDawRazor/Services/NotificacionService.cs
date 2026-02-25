namespace PandaDawRazor.Services;

/// <summary>
/// Modelo de notificación para el sistema de alertas en tiempo real
/// </summary>
public class Notificacion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Tipo { get; set; } = "info"; // success, warning, error, info
    public string Titulo { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public string Icono { get; set; } = "fa-solid fa-info-circle";
    public DateTime Fecha { get; set; } = DateTime.Now;
}

/// <summary>
/// Servicio singleton para comunicar eventos en tiempo real entre
/// Razor Page handlers y los componentes Blazor Server.
/// </summary>
public class NotificacionService
{
    // ── Notificaciones generales (toast) ──────────────────────────
    /// <summary>userId, notificacion</summary>
    public event Action<string, Notificacion>? OnNotificacion;

    public void Enviar(string userId, Notificacion notificacion)
        => OnNotificacion?.Invoke(userId, notificacion);

    public void EnviarATodos(Notificacion notificacion)
        => OnNotificacion?.Invoke("", notificacion);

    // ── Valoraciones en tiempo real ───────────────────────────────
    /// <summary>productoId</summary>
    public event Action<long>? OnNuevaValoracion;

    public void NotificarNuevaValoracion(long productoId)
        => OnNuevaValoracion?.Invoke(productoId);

    // ── Carrito actualizado ───────────────────────────────────────
    /// <summary>userId, totalItems</summary>
    public event Action<string, int>? OnCarritoActualizado;

    public void NotificarCarritoActualizado(string userId, int totalItems)
        => OnCarritoActualizado?.Invoke(userId, totalItems);
}
