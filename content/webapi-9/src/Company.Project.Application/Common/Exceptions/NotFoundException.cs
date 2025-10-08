using Company.Project.Domain.Exceptions;

namespace Company.Project.Application.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base("Not Found", message) { }
}
