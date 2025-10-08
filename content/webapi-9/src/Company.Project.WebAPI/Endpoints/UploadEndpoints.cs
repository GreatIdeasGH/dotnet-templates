using Company.Project.Application.Authorizations.PolicyDefinitions;
using Company.Project.Application.Features.UploadFile;
using Company.Project.WebAPI.Extensions;

namespace Company.Project.WebAPI.Endpoints;

public sealed class UploadEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/uploads").WithTags("uploads").WithOpenApi();

        // POST: api/upload
        group
            .MapPost("", UploadFile)
            .WithName(nameof(UploadFile))
            .WithDescription("Upload a file to the server")
            .WithSummary("Upload a file")
            .Produces<ApiResponse<UploadFileResponse>>()
            .ProducesCommonForbiddenErrors()
            .DisableAntiforgery() // Required for file uploads
            .RequireAuthorization(AppPermissions.Campaign.Manage);
    }

    // POST: api/upload
    public static async Task<IResult> UploadFile(
        [AsParameters] UploadParameters uploadParameters,
        HttpContext httpContext,
        IUploadFileHandler handler,
        IValidator<UploadParameters> validator,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(uploadParameters, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        // Upload the file
        var response = await handler.UploadFileAsync(uploadParameters, httpContext.Request);

        return response.Match(
            data => TypedResults.Ok(response.Value),
            errors => Results.Extensions.Problem(errors)
        );
    }
}
