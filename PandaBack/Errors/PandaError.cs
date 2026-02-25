namespace PandaBack.Errors;

/// <summary>
/// Clase base abstracta para representar errores en el sistema.
/// </summary>
public abstract record PandaError(string Message);

/// <summary>
/// Error utilizado cuando no se encuentra un recurso.
/// </summary>
public record NotFoundError(string Message) : PandaError(Message);

/// <summary>
/// Error utilizado cuando una solicitud es inválida.
/// </summary>
public record BadRequestError(string Message) : PandaError(Message);

/// <summary>
/// Error utilizado cuando hay un conflicto de datos.
/// </summary>
public record ConflictError(string Message) : PandaError(Message);

/// <summary>
/// Error utilizado cuando no hay stock suficiente de un producto.
/// </summary>
public record StockInsuficienteError(string Message) : PandaError(Message);

/// <summary>
/// Error utilizado cuando el carrito está vacío.
/// </summary>
public record CarritoVacioError(string Message) : PandaError(Message);

/// <summary>
/// Error utilizado cuando una operación no está permitida.
/// </summary>
public record OperacionNoPermitidaError(string Message) : PandaError(Message);

