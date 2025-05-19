namespace GreatIdeas.Template.Application.Common.Extensions;

public static partial class LogDefinitions
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "{message}")]
    public static partial void LogToCritical(
        this ILogger logger,
        Exception? exception,
        string message
    );

    #region LogToError

    [LoggerMessage(Level = LogLevel.Error, Message = "{message}")]
    public static partial void LogToError(this ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "{key} - {message}")]
    public static partial void LogToError(this ILogger logger, Guid key, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "{key} - {message}")]
    public static partial void LogToError(this ILogger logger, string key, string message);

    #endregion

    #region LogToInfo

    [LoggerMessage(Level = LogLevel.Information, Message = "{message}")]
    public static partial void LogToInfo(this ILogger logger, string message);

    #endregion

    #region LogToWarning

    [LoggerMessage(Level = LogLevel.Warning, Message = "{message}")]
    public static partial void LogToWarning(this ILogger logger, string message);

    #endregion

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "A task was cancelled when routing {cancelledHandler}"
    )]
    public static partial void TaskCancelled(this ILogger logger, string? cancelledHandler);
}

public static partial class CrudLogDefinitions
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = " fetched {numRecords} {entity} successfully."
    )]
    public static partial void Retrieve(this ILogger logger, int numRecords, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "fetched {entity}: {key} successfully.")]
    public static partial void GetBy(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "fetched {entity}: {key} successfully.")]
    public static partial void GetBy(this ILogger logger, string key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "created {entity}: {key} successfully.")]
    public static partial void Created(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "created {entity} with id: {key}")]
    public static partial void Created(this ILogger logger, long key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "created {entity} with id: {key}")]
    public static partial void Created(this ILogger logger, ulong key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "updated {entity}: {key}")]
    public static partial void Updated(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "updated {entity} successfully")]
    public static partial void Updated(this ILogger logger, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "updated {entity} with id: {key}")]
    public static partial void Updated(this ILogger logger, long key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "uploaded file successfully: {file}.")]
    public static partial void UploadSuccess(this ILogger logger, string file);

    [LoggerMessage(Level = LogLevel.Information, Message = "{message}: {filePath}.")]
    public static partial void Download(this ILogger logger, string message, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "deleted {entity}: {key} successfully.")]
    public static partial void Deleted(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Information, Message = "deleted {entity}: {key} successfully.")]
    public static partial void Deleted(this ILogger logger, long key, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "{entity}: {key} was not found.")]
    public static partial void NotFound(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "{entity}: {key} was not found.")]
    public static partial void NotFound(this ILogger logger, string key, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "{entity}: {key} already exists.")]
    public static partial void Exists(this ILogger logger, string key, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "could not create {entity}.")]
    public static partial void ErrorCreating(this ILogger logger, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "could not delete {entity}: {key}.")]
    public static partial void ErrorDeleting(this ILogger logger, Guid key, string entity);

    [LoggerMessage(Level = LogLevel.Error, Message = "could not update {entity}: {key}.")]
    public static partial void ErrorUpdating(this ILogger logger, Guid key, string entity);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{entity} has been published with: {key}."
    )]
    public static partial void Publish(this ILogger logger, Guid key, string entity);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{entity} has been published with: {key}."
    )]
    public static partial void Publish(this ILogger logger, long key, string entity);

}
