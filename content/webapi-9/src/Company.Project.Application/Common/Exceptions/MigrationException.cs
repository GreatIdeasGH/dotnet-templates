namespace Company.Project.Application.Common.Exceptions;

public sealed class MigrationException(string message) : Exception(message) { }
