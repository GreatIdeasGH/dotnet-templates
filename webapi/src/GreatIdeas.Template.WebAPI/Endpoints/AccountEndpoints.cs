using FluentValidation;
using GreatIdeas.PagedList;
using GreatIdeas.Template.Application;
using GreatIdeas.Template.Application.Common.Params;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.GetPagedUsers;
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
        var group = app.MapGroup(ApiRoutes.AccountEndpoint).WithTags("accounts").WithOpenApi();

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

        // PATCH: api/accounts/{userId}/profile
        group
            .MapPatch("{userId}/profile", UpdateProfile)
            .WithName(nameof(UpdateProfile))
            .WithDescription("Update student user account")
            .WithSummary("Update student user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(AppPermissions.Account.View);

        // UPDATE: api/accounts/{userId}"
        group
            .MapPut("{userId}", UpdateAccount)
            .WithName(nameof(UpdateAccount))
            .WithDescription("Update user account")
            .WithSummary("Update staff user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(AppPermissions.Account.Manage);

        // PUT: api/accounts/{userId}/resetPassword
        group
            .MapPatch("{userId}/resetPassword", ResetPassword)
            .WithName(nameof(ResetPassword))
            .WithDescription("Reset user password")
            .WithSummary("Reset user password")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AppPermissions.Account.View);

        // POST: api/account/login
        group
            .MapPost("login", LoginAccount)
            .WithName(nameof(LoginAccount))
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
