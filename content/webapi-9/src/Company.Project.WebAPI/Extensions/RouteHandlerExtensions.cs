namespace Company.Project.WebAPI.Extensions;

public static class RouteHandlerExtensions
{
    public static RouteHandlerBuilder ProducesCommonErrors(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);
    }

    public static RouteHandlerBuilder ProducesErrorsWithout404(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);
    }

    public static RouteHandlerBuilder ProducesCommonForbiddenErrors(
        this RouteHandlerBuilder builder
    )
    {
        return builder
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden);
    }
}
