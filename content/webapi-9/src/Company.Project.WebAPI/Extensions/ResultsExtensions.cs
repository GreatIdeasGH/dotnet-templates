using System.Diagnostics;

using Company.Project.WebAPI.Extensions;

using ErrorOr;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Company.Project.WebAPI.Extensions;

public static class ResultsExtensions
{
    public static IResult Problem(
        this IResultExtensions resultExtensions,
        IReadOnlyCollection<Error> errors
    )
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        // If there are no errors, return a 500 Internal Server Error
        if (errors?.Count is 0)
        {
            return TypedResults.Problem();
        }

        // If there is only one error, return a Problem
        var firstError = errors!.First();
        return Problem(firstError);
    }

    private static ProblemHttpResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unexpected => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

        // If the error is an Unauthorized error, return a 401 Unauthorized
        if (error.NumericType is StatusCodes.Status401Unauthorized)
        {
            statusCode = StatusCodes.Status401Unauthorized;
        }
        // If the error a bad request, return a 400 Bad Request
        else if (error.NumericType is StatusCodes.Status400BadRequest)
        {
            statusCode = StatusCodes.Status400BadRequest;
        }

        var extensions = new Dictionary<string, object?>
        {
            { "traceId", Activity.Current?.Id },
            { "errorCodes", new[] { error.Code } },
        };

        return TypedResults.Problem(
            detail: error.Description,
            statusCode: statusCode,
            extensions: extensions
        );
    }

    public static ProblemDetails GetProblemDetails(this object endpoint)
    {
        return (
            endpoint?.GetType().GetProperty(nameof(ProblemDetails))?.GetValue(endpoint)
            as ProblemDetails
        )!;
    }
}
