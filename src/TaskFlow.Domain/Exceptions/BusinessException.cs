namespace TaskFlow.Domain.Exceptions;

public class BusinessException : DomainException
{
    public BusinessException(string message) : base(message) { }
}
