namespace PandaBack.Errors;

public abstract record ProductoError(string Message);

public record NotFoundError(string Message) : ProductoError(Message);

public record BadRequestError(string Message) : ProductoError(Message);
