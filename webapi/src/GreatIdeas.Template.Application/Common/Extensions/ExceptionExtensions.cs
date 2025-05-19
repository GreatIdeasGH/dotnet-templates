using GreatIdeas.Template.Application.Common.Errors;

namespace GreatIdeas.Template.Application.Common.Extensions;

public static class ExceptionExtensions
{
    private const string TaskCancelledMessage = "A task was canceled.";

    public static Error LogCriticalUser(
        this Exception exception,
        ILogger logger,
        Activity? activity,
        string user,
        string message
    )
    {
        if (exception.Message.Contains(TaskCancelledMessage))
        {
            logger.TaskCancelled(exception.Source);
        }
        else
        {
            logger.LogToCritical(exception, message);

#if !DEBUG
            SendNotification
                .NotifyAdminWithExceptionLog(
                    exception,
                    ExceptionNotifications.UrgentBugNotification.ToString().SplitCamelCase()
                )
                .ConfigureAwait(false);
#endif
        }

        OtelUserConstants.AddExceptionEvent(user, activity, exception);
        return DomainErrors.Exception("User", message);
    }

    public static Error LogCriticalUser(
        this Exception exception,
        ILogger logger,
        IPublishEndpoint publishEndpoint,
        Activity? activity,
        string user,
        string message
    )
    {
        if (exception.Message.Contains(TaskCancelledMessage))
        {
            logger.TaskCancelled(exception.Source);
        }
        else
        {
            logger.LogToCritical(exception, message);
            // Send notification to admin
        }

        OtelUserConstants.AddExceptionEvent(user, activity, exception);
        return DomainErrors.Exception("User", message);
    }

    public static Error LogCritical(
        this Exception exception,
        ILogger logger,
        Activity? activity,
        string message,
        string entityName
    )
    {
        if (exception.Message.Contains(TaskCancelledMessage))
        {
            logger.TaskCancelled(exception.Source);
        }
        else
        {
            logger.LogToCritical(exception, message);

#if !DEBUG
            SendNotification
                .NotifyAdminWithExceptionLog(
                    exception,
                    ExceptionNotifications.UrgentBugNotification.ToString().SplitCamelCase()
                )
                .ConfigureAwait(false);
#endif
        }

        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.Exception(entityName, message);
    }

    public static Error LogCritical(
        this Exception exception,
        ILogger logger,
        IPublishEndpoint publishEndpoint,
        Activity? activity,
        string message,
        string entityName
    )
    {
        if (exception.Message.Contains(TaskCancelledMessage))
        {
            logger.TaskCancelled(exception.Source);
        }
        else
        {
            logger.LogToCritical(exception, message);
            // Send notification to admin
        }

        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.Exception(entityName, message);
    }

    public static Error LogWarning(
        this Exception exception,
        ILogger logger,
        Activity? activity,
        string item,
        string message
    )
    {
        logger.LogToWarning(message);
        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.Exception(item, message);
    }

    public static Error LogCancelledTask(
        this Exception exception,
        ILogger logger,
        Activity? activity,
        string item
    )
    {
        logger.TaskCancelled(item);
        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.TaskCancelled(item);
    }
}
