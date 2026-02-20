namespace PandaBack.Errors;

public abstract record PandaError(string Message);

public record NotFoundError(string Message) : PandaError(Message);

public record BadRequestError(string Message) : PandaError(Message);
