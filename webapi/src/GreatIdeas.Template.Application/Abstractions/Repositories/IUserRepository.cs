using System.Security.Claims;
using GreatIdeas.Template.Application.Common.Params;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.Domain.Entities;

namespace GreatIdeas.Template.Application.Abstractions.Repositories;

public interface IUserRepository
{
    ValueTask<ApplicationUser?> FindById(string userId);
    ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(string userId);
    ValueTask<IPagedList<UserAccountResponse>> GetPagedUsersAsync(
        PagingParameters pagingParameters,
        CancellationToken cancellationToken
    );
    ValueTask<ErrorOr<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<AccountCreatedResponse>> CreateAccount(
        CreateAccountRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> UpdateProfileAsync(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> UpdateAccountAsync(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> ResetPassword(string userId, PasswordResetRequest request);

    ValueTask<bool> HasAdminRole(string userId);
}
