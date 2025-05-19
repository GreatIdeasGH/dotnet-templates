using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Common.Constants;

public static class OtelUserConstants
{
    public static void AddErrorEvent(string user, Activity? activity, Error errorInfo)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "user", user },
                    { "error.code", errorInfo.Code },
                    { "error.description", errorInfo.Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEvent(
        string user,
        Activity? activity,
        ILogger logger,
        Error errorInfo
    )
    {
        LogDefinitions.LogUserError(logger, user, errorInfo.Description);
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "user", user },
                    { "error.code", errorInfo.Code },
                    { "error.description", errorInfo.Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddExceptionEvent(string user, Activity? activity, Exception exception)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "EXCEPTION",
                tags: new ActivityTagsCollection
                {
                    { "user", user },
                    { "error.message", exception.Message },
                    { "error.description", exception },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddInfoEvent(string user, string message, Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(
            new ActivityEvent(
                "SUCCESS",
                tags: new ActivityTagsCollection { { "message", message }, { "user", user }, }
            )
        );
        activity?.Stop();
    }
}
