using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using System.Diagnostics;
using System.Net;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public static class ErrorHandlerEndpoint
{
    public static void UseApiEndpoints(this WebApplication app)
    {
        app.MapGroup("/")
            .WithOpenApi()
            .MapGet(
                "/",
                () =>
                    "You're running GreatIdeas.Template.WebAPI. Please use /docs to see Swagger API documentation."
            )
            .ExcludeFromDescription();

        app.MapGet("/error", ErrorHandler).ExcludeFromDescription();


    }

    private static IResult ErrorHandler(HttpContext httpContext)
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
