using FluentValidation;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.WebAPI.Extensions;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public sealed class AccountEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.AccountEndpoint)
            .WithTags("accounts")
            .WithOpenApi()
            .RequireAuthorization();

        // GET: api/accounts/{userId}
        group
            .MapGet("", GetAccount)
            .WithName("GetAccount")
            .WithDescription("Get user account")
            .WithSummary("Get user account")
            .Produces<UserAccountResponse>()
            .ProducesCommonForbiddenErrors();

        // POST: api/accounts
        group
            .MapPost("", CreateAccount)
            .WithName("CreateAccount")
            .WithDescription("Create a new user account")
            .WithSummary("Create a new user account")
            .Produces<AccountCreatedResponse>()
            .ProducesCommonErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // PATCH: api/accounts/{userId}/profile
        group
            .MapPatch("profile", UpdateProfile)
            .WithName("UpdateProfile")
            .WithDescription("Update student user account")
            .WithSummary("Update student user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // UPDATE: api/accounts/{userId}"
        group
            .MapPut("", UpdateAccount)
            .WithName("UpdateStaffAccount")
            .WithDescription("Update staff user account")
            .WithSummary("Update staff user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // PUT: api/accounts/{userId}/resetPassword
        group
            .MapPatch("resetPassword", ResetPassword)
            .WithName("ResetPassword")
            .WithDescription("Reset user password")
            .WithSummary("Reset user password")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors();

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


    // GET: api/account/{userId}
    public static async Task<IResult> GetAccount(
        string userId,
        IGetUserAccountHandler handler,
        CancellationToken token
    )
    {
        var response = await handler.GetUserAccount(userId, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }
    
    // POST: api/account
    public static async Task<IResult> CreateAccount(
        [FromBody] CreateAccountRequest model,
        IValidator<CreateAccountRequest> validator,
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

    // PATCH: api/account/profile
    public static async Task<IResult> UpdateProfile(
        string userId,
        [FromBody] ProfileUpdateRequest model,
        IValidator<ProfileUpdateRequest> validator,
        IProfileUpdateHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
            return TypedResults.ValidationProblem(validated.ToDictionary());

        var response = await handler.UpdateProfile(userId, model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // PUT: api/account
    public static async Task<IResult> UpdateAccount(
        string userId,
        [FromBody] AccountUpdateRequest model,
        IValidator<AccountUpdateRequest> validator,
        IUpdateAccountHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
            return TypedResults.ValidationProblem(validated.ToDictionary());

        var response = await handler.UpdateProfile(userId, model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // PATCH: api/account/resetPassword
    public static async Task<IResult> ResetPassword(
        string userId,
        [FromBody] PasswordResetRequest model,
        IValidator<PasswordResetRequest> validator,
        IResetPasswordHandler handler
    )
    {
        var validated = await validator.ValidateAsync(model);
        if (!validated.IsValid)
            return TypedResults.ValidationProblem(validated.ToDictionary());

        var response = await handler.UpdateProfile(userId, model);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
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
}
