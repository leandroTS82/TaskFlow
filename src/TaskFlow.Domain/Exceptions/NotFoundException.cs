namespace TaskFlow.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.") { }
}
