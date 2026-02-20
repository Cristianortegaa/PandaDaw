namespace PandaBack.Errors;

public abstract record PandaError(string Message);

public record NotFoundError(string Message) : PandaError(Message);

public record BadRequestError(string Message) : PandaError(Message);

public record ConflictError(string Message) : PandaError(Message);

public record ValidationError(string Message) : PandaError(Message);

public record StockInsuficienteError(string Message) : PandaError(Message);

public record CarritoVacioError(string Message) : PandaError(Message);

public record OperacionNoPermitidaError(string Message) : PandaError(Message);

