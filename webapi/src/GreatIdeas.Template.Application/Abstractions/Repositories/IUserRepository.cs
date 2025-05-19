using System.Security.Claims;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.Register;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.Domain.Entities;

namespace GreatIdeas.Template.Application.Abstractions.Repositories;

public interface IUserRepository
{
    ValueTask<ApplicationUser?> FindById(string userId);
    ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(string userId);

    ValueTask<IList<Claim>?> GetClaims(ApplicationUser user);

    Task<ErrorOr<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken);

    Task<ErrorOr<AccountCreatedResponse>> CreateAccount(
        CreateAccountRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<SignUpResponse>> RegisterAccount(
        SignUpRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> UpdateProfileAsync(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> UpdateStaffAccountAsync(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    );

    ValueTask<ErrorOr<string>> ResetPassword(string userId, PasswordResetRequest request);

    ValueTask<bool> UpdateStaffClaimsAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken
    );

    ValueTask AddClaim(string userId, string claimType, string claimValue);

    ValueTask AddClaim(ApplicationUser user, string claimType, string claimValue);

    ValueTask RemoveClaim(ApplicationUser user, string claimType, string claimValue);

    ValueTask<bool> UpdateClaim(string userId, string claimType, string claimValue);

    ValueTask<int> HasAdminRole(string userId);
}
