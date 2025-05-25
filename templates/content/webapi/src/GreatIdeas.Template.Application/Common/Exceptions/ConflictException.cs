using GreatIdeas.Template.Domain.Exceptions;

namespace GreatIdeas.Template.Application.Common.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message)
        : base("Item already exists", message) { }
}
