namespace GreatIdeas.Template.Application.Common.Constants;

public static class OtelUserConstants
{
    public static void AddErrorEventByEmail(string email, Activity? activity, Error errorInfo)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "user.email", email },
                    { "error.code", errorInfo.Code },
                    { "error.description", errorInfo.Description }
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEventById(string userId, Activity? activity, Error errorInfo)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "user", userId },
                    { "error.code", errorInfo.Code },
                    { "error.description", errorInfo.Description }
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEvent(
        string user,
        Activity? activity,
        Error errorInfo
    )
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
                    { "error.description", errorInfo.Description }
                }
            )
        );
        activity?.Stop();
    }

    public static void AddExceptionEvent(string email, Activity? activity, Exception exception)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "EXCEPTION",
                tags: new ActivityTagsCollection
                {
                    { "user.email", email },
                    { "error.message", exception.Message },
                    { "error.description", exception }
                }
            )
        );
        activity?.Stop();
    }

    public static void AddExceptionEvent(Activity? activity, Exception exception)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        // Add event
        activity?.AddEvent(
            new ActivityEvent(
                "EXCEPTION",
                tags: new ActivityTagsCollection
                {
                    { "error.message", exception.Message },
                    { "error.description", exception }
                }
            )
        );
        activity?.Stop();
    }

    public static void AddInfoEventWithEmail(string email, string message, Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(
            new ActivityEvent(
                "SUCCESS",
                tags: new ActivityTagsCollection { { "message", message }, { "user.email", email } }
            )
        );
        activity?.Stop();
    }

    public static void AddInfoEventWithUserId(string userId, string message, Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(
            new ActivityEvent(
                "SUCCESS",
                tags: new ActivityTagsCollection { { "message", message }, { "user.id", userId } }
            )
        );
        activity?.Stop();
    }
}