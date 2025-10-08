using Company.Project.Application.Common.Params;
using Company.Project.Application.Features.Account.ChangePassword;
using Company.Project.Application.Features.Account.ConfirmEmail;
using Company.Project.Application.Features.Account.CreateAccount;
using Company.Project.Application.Features.Account.ForgotPassword;
using Company.Project.Application.Features.Account.GetAccount;
using Company.Project.Application.Features.Account.Login;
using Company.Project.Application.Features.Account.Logout;
using Company.Project.Application.Features.Account.RefreshToken;
using Company.Project.Application.Features.Account.ResendEmail;
using Company.Project.Application.Features.Account.ResetPassword;
using Company.Project.Application.Features.Account.UpdateAccount;
using Company.Project.Application.Features.Account.UpdateProfile;
using Company.Project.Application.Responses.Authentication;

using Company.Project.Domain.Entities;

namespace Company.Project.Application.Abstractions.Repositories;

public interface IUserRepository
{
    ValueTask<ApplicationUser?> FindById(string userId);

    ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(
        string userId,
        CancellationToken cancellationToken
    );

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

    ValueTask<ErrorOr<bool>> DeleteAccountAsync(string userId, CancellationToken cancellationToken);

    ValueTask<ErrorOr<string>> ChangePassword(
        string userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> ConfirmEmail(
        ConfirmEmailResponse request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<AccountCreatedResponse>> ResendConfirmEmail(
        ResendEmailRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<RefreshTokenResponse>> RefreshToken(
        RefreshTokenRequest refreshRequest,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<ForgottenPasswordResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> DeactivateAccountAsync(
        string userId,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> ActivateAccountAsync(
        string userId,
        CancellationToken cancellationToken
    );
    ValueTask<ErrorOr<int>> CountUsers(CancellationToken cancellationToken);

    ValueTask<ErrorOr<int>> CountAdmins();

    ValueTask<ErrorOr<LogoutResponse>> LogoutUser(
        Guid sessionId,
        CancellationToken cancellationToken
    );
}
