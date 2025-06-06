using FluentValidation;
using GreatIdeas.PagedList;
using GreatIdeas.Template.Application.Authorizations.PolicyDefinitions;
using GreatIdeas.Template.Application.Common.Params;
using GreatIdeas.Template.Application.Features.Account.ActivateAccount;
using GreatIdeas.Template.Application.Features.Account.ChangePassword;
using GreatIdeas.Template.Application.Features.Account.ConfirmEmail;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.DeleteAccount;
using GreatIdeas.Template.Application.Features.Account.ForgotPassword;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.GetPagedUsers;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.RefreshToken;
using GreatIdeas.Template.Application.Features.Account.ResendEmail;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.WebAPI.Extensions;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public sealed class AccountEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRouteNames.AccountEndpoint).WithTags("accounts").WithOpenApi();

        // POST: api/account/login
        group
            .MapPost("login", LoginAccount)
            .WithName(nameof(LoginAccount))
            .WithDescription("Login to the application with valid credentials")
            .WithSummary("Login to the application")
            .Produces<LoginResponse>()
            .Produces<ApiValidationResponse>(StatusCodes.Status400BadRequest)
            .ProducesCommonErrors();

        // GET: api/accounts/{userId}
        group
            .MapGet("{userId}", GetAccount)
            .WithName(nameof(GetAccount))
            .WithDescription("Get user account")
            .WithSummary("Get user account")
            .Produces<UserAccountResponse>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AppPermissions.Account.View);

        // GET: api/accounts/paged
        group
            .MapGet("paged", GetPagedUsers)
            .WithName(nameof(GetPagedUsers))
            .WithDescription("Get users with pagination")
            .WithSummary("Get users with pagination")
            .Produces<ApiPagingResponse<UserAccountResponse>>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AppPermissions.Account.View);

        // POST: api/accounts
        group
            .MapPost("", CreateAccount)
            .WithName(nameof(CreateAccount))
            .WithDescription("Create a new user account")
            .WithSummary("Create a new user account")
            .Produces<AccountCreatedResponse>()
            .ProducesCommonErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(AppPermissions.Account.Manage);

        // UPDATE: api/accounts/{userId}/profile
        group
            .MapPut("{userId}/profile", UpdateProfile)
            .WithName(nameof(UpdateProfile))
            .WithDescription("Update user account profile")
            .WithSummary("Update user profile")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(AppPermissions.Account.View);

        // UPDATE: api/accounts/{userId}"
        group
            .MapPut("{userId}", UpdateAccount)
            .WithName(nameof(UpdateAccount))
            .WithDescription("Update user account")
            .WithSummary("Update user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(AppPermissions.Account.Manage);

        // POST: api/accounts/{userId}/resetPassword
        group
            .MapPost("{userId}/resetPassword", ResetPassword)
            .WithName(nameof(ResetPassword))
            .WithDescription("Reset user password")
            .WithSummary("Reset user password")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AppPermissions.Account.View);

        // POST: api/account/{userid}/changePassword
        group
            .MapPost("{userId}/changePassword", ChangePassword)
            .WithName(nameof(ChangePassword))
            .WithDescription("Change user password by userId")
            .WithSummary("Change user password")
            .Produces<ApiResponse>()
            .ProducesCommonErrors()
            .RequireAuthorization(AppPermissions.Account.Manage);

        // POST: api/account/refreshtoken
        group
            .MapPost("refreshToken", RefreshToken)
            .WithName(nameof(RefreshToken))
            .WithDescription("Refresh token for a logged in user")
            .WithSummary("Refresh token")
            .Produces<RefreshTokenResponse>()
            .ProducesCommonErrors()
            .RequireAuthorization();

        // POST: api/account/confirmAccount
        group
            .MapPost("confirmAccount", ConfirmAccount)
            .WithName(nameof(ConfirmAccount))
            .WithDescription("Confirm the account of the user")
            .WithSummary("Confirm account")
            .Produces<ApiResponse>()
            .ProducesCommonErrors();

        // POST: api/account/resendConfirmation
        group
            .MapPost("resendConfirmation", ResendConfirmationEmail)
            .WithName(nameof(ResendConfirmationEmail))
            .WithDescription("Resend the confirmation to the email")
            .WithSummary("Resend confirmation")
            .Produces<ApiResponse>()
            .ProducesCommonErrors();

        // POST: api/account/forgotPassword
        group
            .MapPost("forgotPassword", ForgottenPassword)
            .WithName(nameof(ForgottenPassword))
            .WithDescription("Send a forgotten password confirmation to user")
            .WithSummary("Forgotten Password")
            .Produces<ApiResponse>()
            .ProducesCommonErrors();

        //POST: api/account/{userId}/activate
        group
            .MapPost("{userId}/activate", ActivateAccount)
            .WithName(nameof(ActivateAccount))
            .WithDescription("Activate the user account")
            .WithSummary("Activate account")
            .Produces<ApiResponse>()
            .ProducesCommonErrors()
            .RequireAuthorization(AppPermissions.Account.Manage);

        // POST: api/account/{userId}/deactivate
        group
            .MapPost("{userId}/deactivate", DeactivateAccount)
            .WithName(nameof(DeactivateAccount))
            .WithDescription("Deactivate the user account")
            .WithSummary("Deactivate account")
            .Produces<ApiResponse>()
            .ProducesCommonErrors()
            .RequireAuthorization(AppPermissions.Account.Manage);

        // DELETE: api/account/{userId}
        group
            .MapDelete("{userId}", DeleteAccount)
            .WithName(nameof(DeleteAccount))
            .WithDescription("Delete user account")
            .WithSummary("Delete user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AppPermissions.Account.Manage);
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

    // GET: api/account/paged

    public static async Task<IResult> GetPagedUsers(
        [AsParameters] PagingParameters pagingParams,
        IGetPagedUsersHandler handler,
        HttpResponse response,
        CancellationToken token
    )
    {
        var result = await handler.GetPagedUsers(pagingParams, token);
        if (result.IsError)
        {
            return Results.Extensions.Problem(result.Errors);
        }

        var pagination = new PagedListMetaData(result.Value);
        var paged = new ApiPagingResponse<UserAccountResponse>(result.Value, pagination);

        response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
        return TypedResults.Ok(paged);
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
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

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
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

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
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

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
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

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
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var response = await handler.LoginAccountHandler(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // DELETE: api/account
    public static async Task<IResult> DeleteAccount(
        string userId,
        IDeleteAccountHandler handler,
        CancellationToken token
    )
    {
        var response = await handler.DeleteAccount(userId, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/{userid}/changePassword
    public static async Task<IResult> ChangePassword(
        string userId,
        ChangePasswordRequest model,
        IChangePasswordHandler handler,
        IValidator<ChangePasswordRequest> validator,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var result = await handler.ChangePassword(userId, model, token);

        return result.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/refreshtoken
    public static async Task<IResult> RefreshToken(
        RefreshTokenRequest model,
        IValidator<RefreshTokenRequest> validator,
        IRefreshTokenHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var response = await handler.RefreshToken(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/confirmEmail
    public static async Task<IResult> ConfirmAccount(
        ConfirmEmailResponse model,
        IValidator<ConfirmEmailResponse> validator,
        IConfirmEmailHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var response = await handler.ConfirmEmail(model);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/resendConfirmation
    public static async Task<IResult> ResendConfirmationEmail(
        ResendEmailRequest model,
        IValidator<ResendEmailRequest> validator,
        IResendEmailHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var response = await handler.ResendEmail(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/forgotPassword
    public static async Task<IResult> ForgottenPassword(
        ForgotPasswordRequest model,
        IValidator<ForgotPasswordRequest> validator,
        IForgotPasswordHandler handler,
        CancellationToken token
    )
    {
        var validated = await validator.ValidateAsync(model, token);
        if (!validated.IsValid)
        {
            return TypedResults.ValidationProblem(validated.ToDictionary());
        }

        var response = await handler.ForgotPassword(model, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/{userId}/activate
    public static async Task<IResult> ActivateAccount(
        string userId,
        IActivateAccountHandler handler,
        CancellationToken token
    )
    {
        var response = await handler.DeactivateAccount(userId, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }

    // POST: api/account/{userId}/deactivate
    public static async Task<IResult> DeactivateAccount(
        string userId,
        IDeactivateAccountHandler handler,
        CancellationToken token
    )
    {
        var response = await handler.DeactivateAccount(userId, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }
}
