using GreatIdeas.Template.Domain.Exceptions;

namespace GreatIdeas.Template.Application.Common.Exceptions;

public class UnprocessableException : BaseException
{
    public UnprocessableException(string message)
        : base("Unprocessable Request", message) { }
}
