namespace GreatIdeas.Template.Application.Common.Extensions;

static partial class LogDefinitions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User: {key} - {message}")]
    public static partial void LogUserInfo(this ILogger logger, string key, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "User: {key} {message}")]
    public static partial void LogUserError(this ILogger logger, string key, string message);
}
