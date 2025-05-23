using System.Text;
using GreatIdeas.Template.Application.Common.Params;
using GreatIdeas.Template.Application.Features.Account;
using GreatIdeas.Template.Application.Features.Account.ChangePassword;
using GreatIdeas.Template.Application.Features.Account.ConfirmEmail;
using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.ForgotPassword;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.RefreshToken;
using GreatIdeas.Template.Application.Features.Account.ResendEmail;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;
using GreatIdeas.Template.Application.Features.Account.UpdateProfile;
using GreatIdeas.Template.Application.Responses.Authentication;

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

    public async ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(GetUserAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var user = await dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return DomainUserErrors.UserNotFound;
        }

        var userRole = await userManager.GetRolesAsync(user);
        var result = new UserAccountResponse
        {
            FullName = user.FullName!,
            UserId = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            IsActive = user.IsActive,
            Username = user.UserName!,
            Role = userRole[0],
        };
        return result;
    }

    public async ValueTask<ErrorOr<LoginResponse>> Login(
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
                .FirstOrDefaultAsync(cancellationToken);

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

            return new LoginResponse
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
                Activity.Current,
                request.Username,
                "Account login failed"
            );
        }
    }

    public async ValueTask<ErrorOr<AccountCreatedResponse>> CreateAccount(
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

            // Get duplicate phone number
            var phoneNumberExists = await dbContext
                .Users.AsNoTracking()
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
            if (phoneNumberExists)
            {
                var errorPhoneNumber = DomainUserErrors.PhoneNumberExists(request.PhoneNumber);
                logger.LogUserError(request.Email!, errorPhoneNumber.Description);
                return errorPhoneNumber;
            }

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
            await transaction.CreateSavepointAsync("create user", cancellationToken);

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
                await transaction.CreateSavepointAsync("add claims", cancellationToken);

                // Generate code for confirmation
                var code = await userManager.GenerateEmailConfirmationTokenAsync(userEntity);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                logger.LogUserInfo(
                    userEntity.UserName!,
                    "User created a new account with password."
                );

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return new AccountCreatedResponse(userEntity.Id, request.Email!, code);
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
            await transaction.RollbackAsync(cancellationToken);
            return exception.LogCriticalUser(
                logger,
                activity,
                request.Email!,
                "Account creation failed"
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
            // Get user
            var existingUser = await dbContext
                .Users.Where(x => x.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingUser is null)
            {
                var errorNotFound = DomainUserErrors.UserNotFound;
                logger.LogUserError(userId, errorNotFound.Description);
                return errorNotFound;
            }

            // Get duplicate phone number
            var phoneNumberExists = await dbContext
                .Users.AsNoTracking()
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
            if (phoneNumberExists && existingUser.PhoneNumber != request.PhoneNumber)
            {
                var errorPhoneNumber = DomainUserErrors.PhoneNumberExists(request.PhoneNumber);
                logger.LogUserError(userId, errorPhoneNumber.Description);
                return errorPhoneNumber;
            }

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
                        var phoneNumberClaim = new Claim(
                            JwtClaimTypes.PhoneNumber,
                            request.PhoneNumber.Trim()
                        );
                        await userManager.AddClaimsAsync(
                            existingUser!,
                            [nameClaim, phoneNumberClaim]
                        );
                    }
                }

                var message = "User profile updated successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEventWithUserId(userId, message, activity);

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
                activity,
                userId,
                "User profile update failed"
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

        // Get user
        var existingUser = await dbContext
            .Users.Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser is null)
        {
            var errorNotFound = DomainUserErrors.UserNotFound;
            logger.LogUserError(userId, errorNotFound.Description);
            return errorNotFound;
        }

        // Get duplicate phone number
        var phoneNumberExists = await dbContext
            .Users.AsNoTracking()
            .AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

        if (phoneNumberExists && existingUser.PhoneNumber != request.PhoneNumber)
        {
            var errorPhoneNumber = DomainUserErrors.PhoneNumberExists(request.PhoneNumber);
            logger.LogUserError(userId, errorPhoneNumber.Description);
            return errorPhoneNumber;
        }

        existingUser!.Update(request.FullName, request.PhoneNumber, request.IsActive);
        dbContext.Entry(existingUser).State = EntityState.Modified;
        var result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result > 0)
        {
            var claims = await userManager.GetClaimsAsync(existingUser);

            // Update claims
            var claimsToDelete = claims
                .Where(x =>
                    x.Type
                        is JwtClaimTypes.Name
                            or JwtClaimTypes.PhoneNumber
                            or JwtClaimTypes.Email
                            or UserClaims.Username
                )
                .ToList();
            if (claimsToDelete.Count > 0)
            {
                var res = await userManager.RemoveClaimsAsync(existingUser!, claimsToDelete);
                if (res.Succeeded)
                {
                    // Update claims
                    var nameClaim = new Claim(JwtClaimTypes.Name, request.FullName.Trim());
                    var phoneNumberClaim = new Claim(
                        JwtClaimTypes.PhoneNumber,
                        request.PhoneNumber.Trim()
                    );
                    var emailClaim = new Claim(JwtClaimTypes.Email, request.Email.Trim());
                    var usernameClaim = new Claim(UserClaims.Username, request.Username.Trim());
                    await userManager.AddClaimsAsync(
                        existingUser,
                        [nameClaim, phoneNumberClaim, emailClaim, usernameClaim]
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
            OtelUserConstants.AddInfoEventWithUserId(userId, message, activity);
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
                OtelUserConstants.AddInfoEventWithUserId(userId, message, activity);
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
                activity,
                userId,
                "User password reset failed!"
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

    public async ValueTask<ErrorOr<bool>> DeleteAccountAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        // Get user
        var existingUser = await dbContext.Users.FindAsync(userId, cancellationToken);
        if (existingUser is null)
        {
            logger.LogUserError(userId, $"Could not find user {userId}");
            return DomainUserErrors.UserNotFound;
        }

        dbContext.Users.Remove(existingUser);
        var result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result > 0)
        {
            logger.LogUserInfo(userId, "User account deleted successfully.");
            return true;
        }

        var error = DomainUserErrors.UpdateFailed("Could not delete user account");
        logger.LogUserError(userId, error.Description);
        return error;
    }

    public async ValueTask<IPagedList<UserAccountResponse>> GetPagedUsersAsync(
        PagingParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        // Fetch users and include their roles
        var users = FilterUsers(pagingParameters);

        var userList = await users.ToListAsync(cancellationToken);

        var userAccountResponses = new List<UserAccountResponse>();

        foreach (var user in userList)
        {
            var roles = await userManager.GetRolesAsync(user);
            userAccountResponses.Add(
                new UserAccountResponse
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Username = user.UserName!,
                    PhoneNumber = user.PhoneNumber!,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                }
            );
        }

        var response = await userAccountResponses
            .AsQueryable()
            .ToPagedListAsync(pagingParameters.PageNumber, pagingParameters.PageSize);

        return response;
    }

    public async ValueTask<ErrorOr<string>> ChangePassword(
        string userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ChangePassword),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Get user
            var currentUser = await dbContext
                .Users.Where(x => x.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentUser is null)
            {
                // Add event
                logger.NotFound(userId, "User");
                OtelUserConstants.AddErrorEventById(
                    userId,
                    activity,
                    DomainUserErrors.UserNotFound
                );
                return DomainUserErrors.UserNotFound;
            }

            activity?.Stop();

            // Start activity
            using var changePasswordActivity = ActivitySource.CreateActivity(
                "ChangeUserPassword",
                ActivityKind.Server
            );
            changePasswordActivity?.Start();

            var result = await userManager.ChangePasswordAsync(
                currentUser,
                request.OldPassword,
                request.NewPassword
            );
            if (result.Succeeded)
            {
                logger.LogInformation("User {UserId} changed password successfully.", userId);
                OtelUserConstants.AddInfoEventWithUserId(
                    currentUser.Id,
                    "User changed password successfully.",
                    activity
                );
                return "Password changed successfully.";
            }

            var error = DomainUserErrors.PasswordChangeFailed(
                result.Errors.FirstOrDefault()!.Description
            );
            logger.LogError("User: {UserId} password change failed.", userId);
            OtelUserConstants.AddErrorEventById(userId, activity, error);
            return error;
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(logger, activity, userId, "Password change failed");
        }
    }

    public async ValueTask<ErrorOr<string>> ConfirmEmail(
        ConfirmEmailResponse request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ConfirmEmail),
            ActivityKind.Server
        );
        activity?.Start();
        try
        {
            // Get user
            var currentUser = await dbContext
                .Users.Where(x => x.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUser is null)
            {
                // Add event
                logger.NotFound(request.UserId, "User");
                OtelUserConstants.AddErrorEventById(
                    request.UserId,
                    activity,
                    DomainUserErrors.UserNotFound
                );
                return DomainUserErrors.UserNotFound;
            }

            if (currentUser.EmailConfirmed)
            {
                OtelUserConstants.AddInfoEventWithUserId(
                    request.UserId,
                    "User has already confirmed account.",
                    activity
                );
                logger.LogWarning("Account already confirmed: {UserId}", request.UserId);
                return "Account already confirmed. Please login to continue.";
            }

            // decode code
            var decoded = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(request.ConfirmationCode)
            );

            // Confirm email
            var result = await userManager.ConfirmEmailAsync(currentUser, decoded);
            if (result.Succeeded)
            {
                logger.LogInformation(
                    "User: {UserId} confirmed email successfully.",
                    request.UserId
                );
                OtelUserConstants.AddInfoEventWithUserId(
                    request.UserId,
                    "Account confirmed successfully.",
                    activity
                );
                return "Account confirmed successfully.";
            }

            var error = DomainUserErrors.ConfirmEmailFailed(
                "Invalid account confirmation code. Please try again."
            );
            logger.LogError("User: {UserId} account confirmation failed.", request.UserId);
            OtelUserConstants.AddErrorEventById(request.UserId, activity, error);
            return error;
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                request.UserId,
                "Confirm email failed"
            );
        }
    }

    public async ValueTask<ErrorOr<AccountCreatedResponse>> ResendConfirmEmail(
        ResendEmailRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ResendConfirmEmail),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Get user
            var currentUser = await dbContext
                .Users.Where(x => x.Email!.ToLower() == request.Email.ToLower())
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUser is null)
            {
                // Add event
                logger.NotFound(request.Email, "User");
                OtelUserConstants.AddErrorEventByEmail(
                    request.Email,
                    activity,
                    DomainUserErrors.UserNotFound
                );
                return DomainUserErrors.UserNotFound;
            }

            if (!currentUser.EmailConfirmed)
            {
                // Generate code for confirmation
                var code = await userManager.GenerateEmailConfirmationTokenAsync(currentUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                logger.LogInformation(
                    "User: {Email} confirmation email sent successfully.",
                    request.Email
                );
                OtelUserConstants.AddInfoEventWithUserId(
                    currentUser.Id,
                    "Confirmation email sent successfully.",
                    activity
                );

                return new AccountCreatedResponse(currentUser.Id, currentUser.Email!, code);
            }

            return DomainUserErrors.AlreadyConfirmed;
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                request.Email!,
                "Confirmation email resend failed"
            );
        }
    }

    public async ValueTask<ErrorOr<RefreshTokenResponse>> RefreshToken(
        RefreshTokenRequest refreshRequest,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(RefreshToken),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(refreshRequest.AccessToken!);

            var userId = principal.Claims.Single(x => x.Type == JwtClaimTypes.Id).Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshRequest.RefreshToken)
            {
                logger.LogToError("Invalid refresh token");
                var error = Error.Validation("Invalid refresh token");
                OtelConstants.AddErrorEvent(activity, error);
                return error;
            }

            activity?.Stop();

            // Validate token and patch user
            var token = await ValidateTokenAndPatchUser(user, cancellationToken);
            return token;
        }
        catch (Exception exception)
        {
            return exception.LogCritical(logger, activity, "Token refresh failed", "User");
        }
    }

    public async Task<ErrorOr<ForgottenPasswordResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ForgotPassword),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Get user
            var currentUser = await dbContext
                .Users.Where(x => x.Email == request.Email)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentUser is null)
            {
                logger.LogError("Could not find user account: {UserName}", request.Email);
                OtelUserConstants.AddErrorEventByEmail(
                    request.Email,
                    activity,
                    DomainErrors.NotFound("User")
                );
                return DomainErrors.NotFound("User");
            }

            activity?.Stop();

            // Remove the old password and generate a temporal one
            using var generateCodeActivity = ActivitySource.CreateActivity(
                "GeneratePasswordResetToken",
                ActivityKind.Server
            );
            generateCodeActivity?.Start();

            var removePasswordResult = await userManager.RemovePasswordAsync(currentUser);
            if (!removePasswordResult.Succeeded)
            {
                var error = DomainUserErrors.PasswordChangeFailed(
                    removePasswordResult.Errors.FirstOrDefault()!.Description
                );
                logger.LogToError(error.Description);
                OtelUserConstants.AddErrorEventByEmail(request.Email, activity, error);
                return DomainUserErrors.PasswordChangeFailed(
                    "Sorry, we could not reset your password, please try again."
                );
            }

            // Generate a temporal password
            var tempPassword = Guid.NewGuid().ToString().AsSpan()[..8].ToString();
            var addPasswordResult = await userManager.AddPasswordAsync(currentUser, tempPassword);
            if (!addPasswordResult.Succeeded)
            {
                var error = DomainUserErrors.PasswordChangeFailed(
                    addPasswordResult.Errors.FirstOrDefault()!.Description
                );
                logger.LogToError(error.Description);
                OtelUserConstants.AddErrorEventByEmail(request.Email!, activity, error);
                return DomainUserErrors.PasswordChangeFailed(
                    "Sorry, we could not reset your password, please try again."
                );
            }

            logger.LogInformation(
                "User: {UserId} forgotten password reset successfully.",
                currentUser.Id
            );
            OtelUserConstants.AddInfoEventWithUserId(
                currentUser.Id,
                "Forgotten password reset successfully.",
                activity
            );

            return new ForgottenPasswordResponse
            {
                UserId = currentUser.Id,
                PasswordResetToken = tempPassword,
                Email = currentUser.Email!,
            };
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                request.Email!,
                "Forgot password failed"
            );
        }
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
                cancellationToken
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

    private IQueryable<ApplicationUser> FilterUsers(PagingParameters pagingParameters)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(FilterUsers),
            ActivityKind.Server
        );
        activity?.Start();

        var collections = dbContext
            .Users.AsNoTracking()
            .AsNoTracking()
            .TagWith("FilterUsers")
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(pagingParameters.Search))
        {
            var searchPattern = $"%{pagingParameters.Search!.Trim()}%";
            collections = collections.Where(a =>
                EF.Functions.ILike(a.FullName, searchPattern)
                || EF.Functions.ILike(a.PhoneNumber!, searchPattern)
                || EF.Functions.ILike(a.Email!, searchPattern)
            );
        }

        // Sort
        if (pagingParameters.OrderBy is null)
        {
            collections = collections.OrderBy(a => a.FullName);
        }

        return collections;
    }
}
