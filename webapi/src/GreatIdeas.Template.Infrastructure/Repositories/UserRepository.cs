using System.Text;
using GreatIdeas.Template.Application.Features.Account;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.Application.Responses.Authentication;
using Error = ErrorOr.Error;

namespace GreatIdeas.Template.Infrastructure.Repositories;

internal sealed class UserRepository(
    JwtService jwtService,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<UserRepository> logger
) : IUserRepository
{
    private static readonly ActivitySource ActivitySource = new(nameof(UserRepository));
   
    public async ValueTask<ApplicationUser?> FindById(string userId)
    {
        using var activity = ActivitySource.CreateActivity(nameof(FindById), ActivityKind.Server);
        activity?.Start();

        return await dbContext.Users.FindAsync(userId);
    }

    public async ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(string userId)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(GetUserAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return DomainUserErrors.UserNotFound;
        }

        var userRole = await userManager.GetRolesAsync(user);
        var result = new UserAccountResponse()
        {
            UserId = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            IsActive = user.IsActive,
            Username = user.UserName!,
            Role = userRole[0],
        };
        return result;
    }

    public async Task<ErrorOr<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity("GetUser", ActivityKind.Server);
        activity?.Start();
        try
        {
            var currentUser = await dbContext
                .Users.Where(x => x.UserName!.ToLower() == request.Username.ToLower())
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (currentUser is null)
            {
                // Add event
                logger.LogError("Could not find user account: {UserName}", request.Username);
                OtelUserConstants.AddErrorEvent(
                    request.Username,
                    activity,
                    DomainUserErrors.InvalidLoginCredentials
                );
                return DomainUserErrors.InvalidLoginCredentials;
            }

            // Check if user is not confirmed
            if (!currentUser.EmailConfirmed)
            {
                // Add event
                var error = DomainUserErrors.AlreadyConfirmed;
                OtelUserConstants.AddErrorEvent(request.Username, activity, error);
                logger.LogError("User account is not confirmed: {UserName}", request.Username);
                return DomainUserErrors.NotConfirmed;
            }

            // Check if user is active
            if (!currentUser.IsActive)
            {
                // Add event
                var error = DomainUserErrors.InActive;
                OtelUserConstants.AddErrorEvent(request.Username, activity, error);
                logger.LogError("User account is not active: {UserName}", request.Username);
                return error;
            }
            activity?.Stop();

            // Check if password is correct
            using var passwordActivity = ActivitySource.CreateActivity(
                "IsPasswordValid",
                ActivityKind.Server
            );
            passwordActivity?.Start();
            var isPasswordValid = await userManager.CheckPasswordAsync(
                currentUser,
                request.Password
            );
            if (!isPasswordValid)
            {
                // Add event
                logger.LogError("Invalid password attempt: {UserName}", request.Username);
                OtelUserConstants.AddErrorEvent(
                    request.Username,
                    passwordActivity,
                    DomainUserErrors.InvalidLoginCredentials
                );
                return DomainUserErrors.InvalidLoginCredentials;
            }
            passwordActivity?.Stop();

            // Validate token and patch user
            var tokenResponse = await ValidateTokenAndPatchUser(currentUser, cancellationToken);

            // Return response
            logger.LogToInfo($"User: {request.Username} logged in successfully.");

            return new LoginResponse()
            {
                UserId = currentUser.Id,
                AccessToken = tokenResponse.Value.AccessToken,
                RefreshToken = tokenResponse.Value.RefreshToken!,
            };
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity: Activity.Current,
                user: request.Username,
                message: "Account login failed"
            );
        }
    }

    public async Task<ErrorOr<AccountCreatedResponse>> CreateAccount(
        CreateAccountRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(CreateAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // start transaction
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            // Create account
            // activity
            using var createUserActivity = ActivitySource.CreateActivity(
                "CreateUser",
                ActivityKind.Server
            );

            // Create user
            var userExists = await dbContext
                .Users.AsNoTracking()
                .AnyAsync(x => x.Email == request.Email, cancellationToken);
            if (userExists)
            {
                var userExistsMessage = DomainUserErrors.EmailExists(request.Email!);
                OtelUserConstants.AddErrorEvent(request.Username, activity, userExistsMessage);
                return userExistsMessage;
            }

            // Create user
            var userEntity = request.ToUser();
            var result = await userManager.CreateAsync(userEntity, request.Password);

            // createsavepoint: create user
            await transaction.CreateSavepointAsync(
                "create user",
                cancellationToken: cancellationToken
            );

            if (result.Succeeded)
            {
                // Add claims
                using var addClaimsActivity = ActivitySource.CreateActivity(
                    "AddRoleClaims",
                    ActivityKind.Server
                );

                _ = await userManager.AddClaimsAsync(
                    userEntity,
                    [
                        new Claim(JwtClaimTypes.Id, $"{userEntity.Id}"),
                        new Claim(UserClaims.Username, userEntity.UserName!),
                    ]
                );

                // Add user role
                _ = await userManager.AddToRoleAsync(userEntity, request.Role);

                // createsavepoint: add claims
                await transaction.CreateSavepointAsync(
                    "add claims",
                    cancellationToken: cancellationToken
                );

                // Generate code for confirmation
                var code = await userManager.GenerateEmailConfirmationTokenAsync(userEntity);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                logger.LogUserInfo(
                    userEntity.UserName!,
                    "User created a new account with password."
                );

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return new AccountCreatedResponse(
                    Email: request.Email!,
                    VerificationCode: code
                );
            }

            var error = DomainUserErrors.CreationFailed(
                result.Errors.FirstOrDefault()!.Description
            );
            logger.LogToError("username", error.Description);
            OtelUserConstants.AddErrorEvent(request.Username, activity, error);
            return error;
        }
        catch (Exception exception)
        {
            // Rollback transaction
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            return exception.LogCriticalUser(
                logger,
                activity: activity,
                user: request.Email!,
                message: "Account creation failed"
            );
        }
    }

    public async ValueTask<ErrorOr<string>> UpdateProfileAsync(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(UpdateProfileAsync),
            ActivityKind.Server
        );
        activity?.Start();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingUser = await dbContext
                .Users.Where(x => x.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            existingUser!.Update(request.FullName, request.PhoneNumber);
            dbContext.Entry(existingUser).State = EntityState.Modified;
            var result = await dbContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                var claims = await userManager.GetClaimsAsync(existingUser!);

                // Update claims
                var claimsToDelete = claims
                    .Where(x => x.Type is JwtClaimTypes.Name or JwtClaimTypes.PhoneNumber)
                    .ToList();
                if (claimsToDelete.Count > 0)
                {
                    var res = await userManager.RemoveClaimsAsync(existingUser!, claimsToDelete);
                    if (res.Succeeded)
                    {
                        // Update claims
                        var nameClaim = new Claim(JwtClaimTypes.Name, request.FullName.Trim());
                        var phoneNumberClaim = new Claim(JwtClaimTypes.PhoneNumber, request.PhoneNumber.Trim());
                        await userManager.AddClaimsAsync(
                            existingUser!,[nameClaim, phoneNumberClaim]
                        );
                    }
                }

                var message = "User profile updated successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEvent(userId, message, activity);

                await transaction.CommitAsync();
                return message;
            }

            var error = DomainUserErrors.UpdateFailed("Could not update user profile");
            logger.LogUserError(userId, error.Description);
            return error;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return exception.LogCriticalUser(
                logger,
                activity: activity,
                user: userId,
                message: "User profile update failed"
            );
        }
    }

    // Update account
    public async ValueTask<ErrorOr<string>> UpdateAccountAsync(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(UpdateAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var existingUser = await dbContext
            .Users.Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser is not null)
        {
            var claims = await userManager.GetClaimsAsync(existingUser);

            // Update claims
            var claimsToDelete = claims
                .Where(x => x.Type is JwtClaimTypes.Name or JwtClaimTypes.PhoneNumber or JwtClaimTypes.Email or UserClaims.Username)
                .ToList();
            if (claimsToDelete.Count > 0)
            {
                var res = await userManager.RemoveClaimsAsync(existingUser!, claimsToDelete);
                if (res.Succeeded)
                {
                    // Update claims
                    var nameClaim = new Claim(JwtClaimTypes.Name, request.FullName.Trim());
                    var phoneNumberClaim = new Claim(JwtClaimTypes.PhoneNumber, request.PhoneNumber.Trim());
                    var emailClaim = new Claim(JwtClaimTypes.Email, request.Email.Trim());
                    var usernameClaim = new Claim(UserClaims.Username, request.Username.Trim());
                    await userManager.AddClaimsAsync(
                        existingUser,[nameClaim, phoneNumberClaim, emailClaim, usernameClaim]
                    );
                }
            }
            
            // Update role
                var userRoles = await userManager.GetRolesAsync(existingUser);
                if (userRoles.Any())
                {
                    var existingRole = userRoles.FirstOrDefault(x => x == request.Role)!;

                    // if existing role and request role are different, update role
                    if (userRoles.Count > 0 && string.IsNullOrEmpty(existingRole))
                    {
                        await userManager.RemoveFromRoleAsync(existingUser, existingRole);
                        await userManager.AddToRoleAsync(existingUser!, request.Role);
                    }
                }

                var message = "Updated account successfully.";
            logger.LogUserInfo(userId, message);
            OtelUserConstants.AddInfoEvent(userId, message, activity);
            return message;
        }

        var error = DomainUserErrors.UpdateFailed("Could not update account");
        logger.LogUserError(userId, error.Description);
        OtelUserConstants.AddErrorEvent(userId, activity, error);
        return string.Empty;
    }

    public async ValueTask<ErrorOr<string>> ResetPassword(
        string userId,
        PasswordResetRequest request
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ResetPassword),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return DomainUserErrors.UserNotFound;
            }
            await userManager.RemovePasswordAsync(user);
            var result = await userManager.AddPasswordAsync(user, request.NewPassword);

            if (result.Succeeded)
            {
                var message = "User password reset successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEvent(userId, message, activity);
                return message;
            }

            var error = DomainUserErrors.UpdateFailed(
                "Could not reset user password, please try again"
            );
            logger.LogUserError(userId, error.Description);
            return error;
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity: activity,
                user: userId,
                message: "User password reset failed!"
            );
        }
    }

    public async ValueTask<bool> HasAdminRole(string userId)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(HasAdminRole),
            ActivityKind.Server
        );
        activity?.Start();

        var users = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
        return users.Any(x => x.Id == userId);
    }

    private async Task<ErrorOr<RefreshTokenResponse>> ValidateTokenAndPatchUser(
        ApplicationUser currentUser,
        CancellationToken cancellationToken
    )
    {
        using var tokenActivity = ActivitySource.CreateActivity(
            "ValidateRefreshToken",
            ActivityKind.Server
        );
        tokenActivity?.Start();

        // Validate refresh token
        var tokenResponse = await jwtService.ValidateRefreshToken(currentUser);

        // Update refresh token
        currentUser.RefreshToken = tokenResponse.RefreshToken;
        currentUser.RefreshTokenExpiryTime = tokenResponse.Expires;
        tokenActivity?.Stop();

        using var patchUserActivity = ActivitySource.CreateActivity(
            "PatchLoginUser",
            ActivityKind.Server
        );
        patchUserActivity?.Start();
        var result = await dbContext
            .Users.Where(x => x.Id == currentUser.Id)
            .TagWith("PatchLoginUser")
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(u => u.RefreshToken, tokenResponse.RefreshToken)
                        .SetProperty(u => u.RefreshTokenExpiryTime, tokenResponse.Expires),
                cancellationToken: cancellationToken
            );

        if (result <= 0)
        {
            var error = Error.Failure("Sorry, request failed, please try again.");
            logger.LogError("User: {UserId} token patch failed.", currentUser.Id);
            OtelUserConstants.AddErrorEvent(currentUser.UserName!, tokenActivity, error);
            return error;
        }

        return tokenResponse;
    }
    
}
