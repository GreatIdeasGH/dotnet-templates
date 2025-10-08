using Company.Project.Domain.Exceptions;

namespace Company.Project.Application.Common.Exceptions;

public class UnprocessableException : BaseException
{
    public UnprocessableException(string message)
        : base("Unprocessable Request", message) { }
}
