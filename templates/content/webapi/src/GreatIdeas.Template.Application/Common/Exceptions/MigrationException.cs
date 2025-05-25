namespace GreatIdeas.Template.Application.Common.Exceptions;

public sealed class MigrationException(string message) : Exception(message)
{
}