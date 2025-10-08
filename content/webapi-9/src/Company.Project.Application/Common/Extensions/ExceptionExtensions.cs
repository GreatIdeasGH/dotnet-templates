using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Common.Extensions;

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
        }

        OtelUserConstants.AddExceptionEvent(user, activity, exception);
        return DomainErrors.Exception("User", message);
    }

    public static Error LogCriticalUser(
        this Exception exception,
        ILogger logger,
        ExceptionNotificationService exceptionNotificationService,
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
            // Fire-and-forget notification - don't block main flow.
            // Notifications are skipped in debug builds to avoid spamming developers during local testing.
            ScheduleNotification(exception, exceptionNotificationService);
#endif
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
        }

        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.Exception(entityName, message);
    }

    public static Error LogCritical(
        this Exception exception,
        ILogger logger,
        ExceptionNotificationService exceptionNotificationService,
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
            // Fire-and-forget notification - don't block main flow
            ScheduleNotification(exception, exceptionNotificationService);
#endif
        }

        OtelConstants.AddExceptionEvent(activity, exception);
        return DomainErrors.Exception(entityName, message);
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

    // disable warning

    public static void ScheduleNotification(
        Exception exception,
        ExceptionNotificationService exceptionNotificationService
    )
    {
        Serilog.Log.Information("Scheduling exception notification...");

        // Fire-and-forget: intentionally not awaited or tracked.
        // Exceptions are suppressed to avoid impacting the main flow.
        // Unhandled exceptions in this task will not be observed.
        _ = Task.Run(async () =>
        {
            try
            {
                await exceptionNotificationService.NotifyAdminWithExceptionLog(
                    exception,
                    ExceptionNotifications.UrgentBugNotification
                );
            }
            catch (Exception ex)
            {
                // Suppress exceptions from notification to prevent cascading failures.
                Serilog.Log.Fatal(ex, "Exception suppressed in ScheduleNotification task.");
            }
        });
    }
}
