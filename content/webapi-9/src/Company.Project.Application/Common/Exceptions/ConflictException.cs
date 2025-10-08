using Company.Project.Domain.Exceptions;

namespace Company.Project.Application.Common.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base("Item already exists", message) { }
}
