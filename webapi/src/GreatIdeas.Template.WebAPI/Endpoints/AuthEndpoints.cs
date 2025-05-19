using FluentValidation;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.Register;
using GreatIdeas.Template.WebAPI.Extensions;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public sealed class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.AuthEndpoint).WithTags("auth").WithOpenApi();

        // POST: api/accounts/register
        group
            .MapPost("register", RegisterAccount)
            .WithName("RegisterAccount")
            .WithDescription("Register a new account")
            .WithSummary("Register a new account")
            .Produces<SignUpResponse>()
            .ProducesCommonErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // POST: api/account/login
        group
            .MapPost("login", LoginAccount)
            .WithName("Login")
            .WithDescription("Login to the application with valid credentials")
            .WithSummary("Login to the application")
            .Produces<LoginResponse>()
            .Produces<ApiValidationResponse>(StatusCodes.Status400BadRequest)
            .ProducesCommonErrors();
    }

    // POST: api/account/login
    public static async Task<IResult> LoginAccount(
        LoginRequest model,
        IValidator<LoginRequest> validator,
        IAccountLoginHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
            return TypedResults.ValidationProblem(validated.ToDictionary());

        var response = await handler.LoginAccountHandler(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/register
    public static async Task<IResult> RegisterAccount(
        [FromBody] SignUpRequest model,
        IValidator<SignUpRequest> validator,
        IAccountCreationHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
            return TypedResults.ValidationProblem(validated.ToDictionary());

        var response = await handler.RegisterAccountHandler(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }
}
