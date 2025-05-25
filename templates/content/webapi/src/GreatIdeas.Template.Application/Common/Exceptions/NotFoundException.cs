using GreatIdeas.Template.Domain.Exceptions;

namespace GreatIdeas.Template.Application.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base("Not Found", message) { }
}
