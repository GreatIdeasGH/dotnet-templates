
namespace Company.Project.Application.Common.Constants;

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

    private static readonly string ErrorCode = "error.code";
    private static readonly string ErrorDescription = "error.description";

    public static void AddErrorEvent<T>(Activity? activity, ErrorOr<T> response)
    {
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                response.Errors[0].Code,
                tags: new ActivityTagsCollection
                {
                    { ErrorCode, response.Errors[0].Code },
                    { ErrorDescription, response.Errors[0].Description },
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
                    { ErrorCode, error.Code },
                    { ErrorDescription, error.Description },
                }
            )
        );
        activity?.Stop();
    }

    public static void AddErrorEvent(ILogger logger, Activity? activity, Error error)
    {
        LogDefinitions.LogToError(logger, error.Description);
        // Set status as error
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddEvent(
            new ActivityEvent(
                "ERROR",
                tags: new ActivityTagsCollection
                {
                    { ErrorCode, error.Code },
                    { ErrorDescription, error.Description },
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
                    { ErrorCode, exception.Message },
                    { ErrorDescription, exception },
                }
            )
        );
        activity?.Stop();

        // Send email to dev team
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
                    { ErrorCode, message },
                    { ErrorDescription, exception },
                }
            )
        );
        activity?.Stop();
        throw exception;
    }

    public static void AddSuccessEvent(Activity? activity, string message)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddEvent(new ActivityEvent("SUCCESS", tags: new() { { "message", message } }));
        activity?.Stop();
    }

    public static void AddSuccessEvent(
        ILogger logger,
        Activity? activity,
        string message,
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
        ILogger logger,
        Activity? activity,
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
