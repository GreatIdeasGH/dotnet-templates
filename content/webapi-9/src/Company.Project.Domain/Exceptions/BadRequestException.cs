namespace Company.Project.Domain.Exceptions;

public class BadRequestException : BaseException
{
    public BadRequestException(string message)
        : base("Bad Request", message) { }
}
