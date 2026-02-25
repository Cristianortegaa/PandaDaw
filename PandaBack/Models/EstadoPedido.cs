namespace PandaBack.Models;

/// <summary>
/// Enum que representa los estados posibles de un pedido.
/// </summary>
public enum EstadoPedido
{
    /// <summary>
    /// Pedido pendiente de procesamiento.
    /// </summary>
    Pendiente,
    
    /// <summary>
    /// Pedido en proceso de preparación.
    /// </summary>
    Procesando,
    
    /// <summary>
    /// Pedido enviado al cliente.
    /// </summary>
    Enviado,
    
    /// <summary>
    /// Pedido entregado al cliente.
    /// </summary>
    Entregado,
    
    /// <summary>
    /// Pedido cancelado.
    /// </summary>
    Cancelado
}