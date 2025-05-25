using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace GreatIdeas.Template.WebAPI.Extensions;

public static class ExceptionHandlerExtension
{
    public static void UseCustomExceptionHandlers(this WebApplication app)
    {
        //app.MapGet("/error", ErrorHandler).ExcludeFromDescription()

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.ContentType = Text.Plain;

                var detail = "Invalid input";
                var statusCode = HttpStatusCode.InternalServerError;
                Dictionary<string, object?> problemDetailsExtensions = [];

                if (
                    context.RequestServices.GetService<IProblemDetailsService>() is
                    { } problemDetailsService
                )
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandlerFeature?.Error;
                    if (exception != null)
                    {
                        problemDetailsExtensions = new Dictionary<string, object?>
                        {
                            { "traceId", Activity.Current?.Id },
                            { "errorCodes", new[] { exception.GetType().Name } },
                        };

                        (statusCode, var message) = exception switch
                        {
                            FileNotFoundException => (
                                HttpStatusCode.NotFound,
                                "The file was not found."
                            ),
                            ValidationException => (HttpStatusCode.BadRequest, exception.Message),
                            PostgresException => (
                                HttpStatusCode.UnprocessableEntity,
                                exception.Message
                            ),
                            DbUpdateException => (
                                HttpStatusCode.UnprocessableEntity,
                                exception.Message
                            ),
                            BadHttpRequestException => (
                                HttpStatusCode.BadRequest,
                                exception.Message
                            ),
                            InvalidOperationException => (
                                HttpStatusCode.BadRequest,
                                "Could not complete the operation. Please try again later."
                            ),
                            TaskCanceledException => (
                                HttpStatusCode.BadRequest,
                                "The request was cancelled."
                            ),
                            FormatException => (HttpStatusCode.BadRequest, exception.Message),
                            NpgsqlException => (
                                HttpStatusCode.UnprocessableEntity,
                                exception.Message
                            ),
                            FileLoadException => (
                                HttpStatusCode.UnprocessableEntity,
                                exception.Message
                            ),
                            _ => (
                                HttpStatusCode.InternalServerError,
                                "An unexpected error occurred."
                            ),
                        };
                        detail = message;
                    }

                    context.Response.StatusCode = (int)statusCode;
                    Log.Fatal(exception, detail);

                    await problemDetailsService.WriteAsync(
                        new ProblemDetailsContext
                        {
                            HttpContext = context,
                            ProblemDetails =
                            {
                                Detail = detail,
                                Status = (int)statusCode,
                                Extensions = problemDetailsExtensions!,
                            },
                        }
                    );
                }
            });
        });
    }

    public static ProblemHttpResult ErrorHandler(HttpContext httpContext)
    {
        using var activity = Activity.Current;
        var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        var problemDetailsExtensions = new Dictionary<string, object?>
        {
            { "traceId", Activity.Current?.Id },
            { "errorCodes", new[] { exception?.GetType().Name } },
        };

        (HttpStatusCode statusCode, string message) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, exception.Message),
            PostgresException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            DbUpdateException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            BadHttpRequestException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException
                => (
                    HttpStatusCode.BadRequest,
                    "Could not complete the operation. Please try again later."
                ),
            TaskCanceledException => (HttpStatusCode.BadRequest, "The request was cancelled."),
            FormatException => (HttpStatusCode.BadRequest, exception.Message),
            NpgsqlException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            FileLoadException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        // log exception
        Log.Fatal(exception, message!);

        return TypedResults.Problem(
            detail: message,
            statusCode: (int)statusCode,
            extensions: problemDetailsExtensions
        );
    }
}

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomExceptionHandler> logger;

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var exceptionMessage = "An unexpected error occurred.";
        logger.LogError(
            "Error Message: {ExceptionMessage}, Time of occurrence {Time}",
            exceptionMessage,
            DateTime.UtcNow
        );
        // Return false to continue with the default behavior
        // - or - return true to signal that this exception is handled
        return ValueTask.FromResult(false);
    }
}


