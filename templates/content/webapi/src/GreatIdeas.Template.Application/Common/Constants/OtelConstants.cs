using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Common.Constants;

public static class OtelConstants
{
    public static readonly string ServiceName = Assembly
        .GetEntryAssembly()!
        .GetName()
        .Name!.ToLower();

    public static readonly string ServiceVersion = Assembly
        .GetEntryAssembly()!
        .GetName()
        .Version!.ToString();

    public static void AddErrorEvent<T>(Activity? activity, ErrorOr<T> response)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                response.Errors[0].Code,
                tags: new ActivityTagsCollection
                {
                    { "error.code", response.Errors[0].Code },
                    { "error.description", response.Errors[0].Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEvent(Activity? activity, Error error)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "error.code", error.Code },
                    { "error.description", error.Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEvent(Activity? activity, ILogger logger, Error error)
    {
        LogDefinitions.LogToError(logger, error.Description);
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { "error.code", error.Code },
                    { "error.description", error.Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddExceptionEvent(Activity? activity, Exception exception)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                "EXCEPTION",
                tags: new ActivityTagsCollection
                {
                    { "error.message", exception.Message },
                    { "error.description", exception },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddExceptionEvent(Activity? activity, Exception exception, string message)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                "EXCEPTION",
                tags: new ActivityTagsCollection
                {
                    { "error.message", message },
                    { "error.description", exception },
                }
            )
        );
        activity?.Stop();
        throw exception;
    }

    public static void AddSuccessEvent(string message, Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent("SUCCESS", tags: new() { { "message", message } }));
        activity?.Stop();
    }

    public static void AddSuccessEvent(
        string message,
        ILogger logger,
        Activity? activity,
        params object[]? values
    )
    {
        LogDefinitions.LogToInfo(logger, message);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent("SUCCESS", tags: new() { { "message", message } }));
        activity?.AddEvent(new ActivityEvent("SUCCESS", tags: new() { { "values", values } }));
        activity?.Stop();
    }

    public static void AddInfoEvent(Activity? activity, string message, params object[]? values)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent("INFO", tags: new() { { "message", message } }));
        activity?.AddEvent(new ActivityEvent("INFO", tags: new() { { "values", values } }));
        activity?.Stop();
    }

    public static void AddInfoEvent(
        Activity? activity,
        ILogger logger,
        string message,
        params object[]? values
    )
    {
        LogDefinitions.LogToInfo(logger, message);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent("INFO", tags: new() { { "message", message } }));
        activity?.AddEvent(new ActivityEvent("INFO", tags: new() { { "values", values } }));
        activity?.Stop();
    }
}
