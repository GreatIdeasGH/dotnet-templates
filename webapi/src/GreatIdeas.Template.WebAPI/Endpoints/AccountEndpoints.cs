using FluentValidation;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
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

        // PATCH: api/accounts/{userId}/profile
        group
            .MapPatch("profile", UpdateProfile)
            .WithName("UpdateProfile")
            .WithDescription("Update student user account")
            .WithSummary("Update student user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // PATCH: api/accounts/{userId}/staff"
        group
            .MapPatch("staff", UpdateStaffAccount)
            .WithName("UpdateStaffAccount")
            .WithDescription("Update staff user account")
            .WithSummary("Update staff user account")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors()
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        // PUT: api/accounts/{userId}/resetPassword
        group
            .MapPut("resetPassword", ResetPassword)
            .WithName("ResetPassword")
            .WithDescription("Reset user password")
            .WithSummary("Reset user password")
            .Produces<ApiResponse>()
            .ProducesCommonForbiddenErrors();
    }

    // PATCH: api/account/profile
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

    // PATCH: api/account/profile
    public static async Task<IResult> UpdateStaffAccount(
        string userId,
        [FromBody] AccountUpdateRequest model,
        IValidator<AccountUpdateRequest> validator,
        IStaffAccountUpdateHandler handler,
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

    // PUT: api/account/resetPassword
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
}
