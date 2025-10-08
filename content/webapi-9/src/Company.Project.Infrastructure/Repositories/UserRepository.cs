using System.Text;

using Company.Project.Infrastructure.Data;

using Company.Project.Application.Common.Params;
using Company.Project.Application.Features.Account;
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

namespace Company.Project.Infrastructure.Repositories;

internal sealed class UserRepository(
    JwtService jwtService,
    ApplicationDbContext dbContext,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    UserManager<ApplicationUser> userManager,
    ILogger<UserRepository> logger,
    IUserSessionRepository userSessionService,
    ITenantService tenantService
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
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context
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
                .Users.Where(x => EF.Functions.ILike(x.UserName!, $"%{request.Username.Trim()}%"))
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUser is null)
            {
                // Add event
                logger.LogToError(
                    request.Username,
                    $"Could not find user account: {request.Username}"
                );
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
                logger.LogToError(
                    request.Username,
                    $"User account is not confirmed: {request.Username}"
                );
                return DomainUserErrors.NotConfirmed;
            }

            // Check if user is active
            if (!currentUser.IsActive)
            {
                // Add event
                var error = DomainUserErrors.InActive;
                OtelUserConstants.AddErrorEvent(request.Username, activity, error);
                logger.LogToError(
                    request.Username,
                    $"User account is not active: {request.Username}"
                );
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
                logger.LogToError(
                    request.Username,
                    $"Invalid password attempt: {request.Username}"
                );
                OtelUserConstants.AddErrorEvent(
                    request.Username,
                    passwordActivity,
                    DomainUserErrors.InvalidLoginCredentials
                );
                return DomainUserErrors.InvalidLoginCredentials;
            }

            passwordActivity?.Stop();

            // Track IP address and create session
            var ipAddress = await tenantService.GetIpAddress();

            var session = await userSessionService.CreateSessionAsync(
                currentUser.Id,
                ipAddress,
                tenantService.UserAgent,
                cancellationToken
            );

            // Validate token and patch user
            var tokenResponse = await ValidateTokenAndPatchUser(currentUser, cancellationToken);

            // Return response
            logger.LogToInfo($"User: {request.Username} logged in successfully.");

            return new LoginResponse
            {
                UserId = currentUser.Id,
                AccessToken = tokenResponse.Value.AccessToken,
                RefreshToken = tokenResponse.Value.RefreshToken!,
                SessionId = session.Id,
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

        await using var context = await dbContextFactory.CreateDbContextAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Get user
            var existingUser = await userManager.FindByIdAsync(userId);

            if (existingUser is null)
            {
                var errorNotFound = DomainUserErrors.UserNotFound;
                logger.LogUserError(userId, errorNotFound.Description);
                return errorNotFound;
            }

            // Check for duplicate phone number (exclude current user)
            var phoneNumberExists = await context
                .Users.AsNoTracking()
                .AnyAsync(
                    x => x.PhoneNumber == request.PhoneNumber && x.Id != userId,
                    cancellationToken
                );

            if (phoneNumberExists)
            {
                var errorPhoneNumber = DomainUserErrors.PhoneNumberExists(request.PhoneNumber);
                logger.LogUserError(userId, errorPhoneNumber.Description);
                return errorPhoneNumber;
            }

            existingUser.Update(request.FullName, request.PhoneNumber);
            var result = await userManager.UpdateAsync(existingUser);

            if (result.Succeeded)
            {
                var claims = await userManager.GetClaimsAsync(existingUser);

                // Update claims
                var claimsToDelete = claims
                    .Where(x => x.Type is JwtClaimTypes.Name or JwtClaimTypes.PhoneNumber)
                    .ToList();
                if (claimsToDelete.Count > 0)
                {
                    var removeResult = await userManager.RemoveClaimsAsync(
                        existingUser,
                        claimsToDelete
                    );
                    if (removeResult.Succeeded)
                    {
                        // Add updated claims
                        var newClaims = new[]
                        {
                            new Claim(JwtClaimTypes.Name, request.FullName.Trim()),
                            new Claim(JwtClaimTypes.PhoneNumber, request.PhoneNumber.Trim()),
                        };
                        await userManager.AddClaimsAsync(existingUser, newClaims);
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

        // Start transactions
        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get user for phone number validation (no tracking needed)
            var phoneNumberExists = await dbContext
                .Users.AsNoTracking()
                .AnyAsync(
                    x => x.PhoneNumber == request.PhoneNumber && x.Id != userId,
                    cancellationToken
                );

            if (phoneNumberExists)
            {
                var errorPhoneNumber = DomainUserErrors.PhoneNumberExists(request.PhoneNumber);
                logger.LogUserError(userId, errorPhoneNumber.Description);
                return errorPhoneNumber;
            }

            // Get user through UserManager to avoid tracking conflicts
            var existingUser = await userManager.FindByIdAsync(userId);
            if (existingUser is null)
            {
                var errorNotFound = DomainUserErrors.UserNotFound;
                logger.LogUserError(userId, errorNotFound.Description);
                return errorNotFound;
            }

            // Update personal details using UserManager
            existingUser.Update(request.FullName, request.PhoneNumber);
            var updateResult = await userManager.UpdateAsync(existingUser);

            if (updateResult.Succeeded)
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
                    var removeClaimsResult = await userManager.RemoveClaimsAsync(
                        existingUser,
                        claimsToDelete
                    );
                    if (removeClaimsResult.Succeeded)
                    {
                        // Add updated claims
                        var newClaims = new[]
                        {
                            new Claim(JwtClaimTypes.Name, request.FullName.Trim()),
                            new Claim(JwtClaimTypes.PhoneNumber, request.PhoneNumber.Trim()),
                            new Claim(JwtClaimTypes.Email, request.Email.Trim()),
                            new Claim(UserClaims.Username, request.Username.Trim()),
                        };
                        await userManager.AddClaimsAsync(existingUser, newClaims);
                    }
                }

                // Update role
                var userRoles = await userManager.GetRolesAsync(existingUser);
                if (userRoles.Any())
                {
                    var currentRole = userRoles.FirstOrDefault();

                    // if existing role and request role are different, update role
                    if (!string.IsNullOrEmpty(currentRole) && currentRole != request.Role)
                    {
                        await userManager.RemoveFromRoleAsync(existingUser, currentRole);
                        await userManager.AddToRoleAsync(existingUser, request.Role);
                    }
                }

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                var message = "Updated account successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEventWithUserId(userId, message, activity);
                return message;
            }

            var error = DomainUserErrors.UpdateFailed("Could not update account");
            logger.LogUserError(userId, error.Description);
            OtelUserConstants.AddErrorEvent(userId, activity, error);

            await transaction.RollbackAsync(cancellationToken);
            return error;
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return exception.LogCriticalUser(logger, activity, userId, "Account update failed");
        }
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
        var existingUser = await userManager.FindByIdAsync(userId);
        if (existingUser is null)
        {
            logger.LogUserError(userId, $"Could not find user {userId}");
            return DomainUserErrors.UserNotFound;
        }

        // Check if user is admin and if admin count is more than 1
        var userRoles = await userManager.GetRolesAsync(existingUser);
        if (userRoles.Contains(UserRoles.Admin))
        {
            var adminUsers = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
            if (adminUsers.Count <= 1)
            {
                var error = DomainUserErrors.DeleteFailed("Cannot delete the only admin account.");
                logger.LogUserError(userId, error.Description);
                OtelUserConstants.AddErrorEventById(userId, activity, error);
                return error;
            }
        }

        dbContext.Users.Remove(existingUser);
        var result = await dbContext.SaveChangesAsync(cancellationToken);

        if (result > 0)
        {
            logger.LogUserInfo(userId, $"{existingUser.FullName}'s account deleted successfully.");
            return true;
        }

        var deleteError = DomainUserErrors.UpdateFailed("Could not delete user account");
        logger.LogUserError(userId, deleteError.Description);
        return deleteError;
    }

    public async ValueTask<IPagedList<UserAccountResponse>> GetPagedUsersAsync(
        PagingParameters pagingParameters,
        CancellationToken cancellationToken
    )
    {
        // Fetch users and include their roles
        var users = FilterUsers(pagingParameters);

        var userList = await users.AsSplitQuery().ToListAsync(cancellationToken);

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
            var currentUser = await userManager.FindByIdAsync(userId);
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
            var currentUser = await userManager.FindByEmailAsync(request.Email);

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
                var error = DomainErrors.UpdateFailed("Invalid refresh token", "RefreshToken");
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

    public async ValueTask<ErrorOr<ForgottenPasswordResponse>> ForgotPassword(
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
            var currentUser = await userManager.FindByEmailAsync(request.Username);
            if (currentUser is null)
            {
                logger.LogError("Could not find user account: {UserName}", request.Username);
                OtelUserConstants.AddErrorEventByEmail(
                    request.Username,
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
                OtelUserConstants.AddErrorEventByEmail(request.Username, activity, error);
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
                OtelUserConstants.AddErrorEventByEmail(request.Username!, activity, error);
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
                FullName = currentUser.FullName!,
            };
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                request.Username!,
                "Forgot password failed"
            );
        }
    }

    public async ValueTask<ErrorOr<string>> DeactivateAccountAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(DeactivateAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Get user
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser is null)
            {
                logger.LogError("Could not find user account: {UserId}", userId);
                OtelUserConstants.AddErrorEventById(
                    userId,
                    activity,
                    DomainErrors.NotFound("User")
                );
                return DomainErrors.NotFound("User");
            }

            // Check if user is admin and if admin count is more than 1
            var userRoles = await userManager.GetRolesAsync(currentUser);
            if (userRoles.Contains(UserRoles.Admin))
            {
                var adminUsers = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
                if (adminUsers.Count <= 1)
                {
                    var error = DomainUserErrors.UpdateFailed(
                        "Cannot deactivate the only admin account."
                    );
                    logger.LogUserError(userId, error.Description);
                    OtelUserConstants.AddErrorEventById(userId, activity, error);
                    return error;
                }
            }

            // Deactivate account
            currentUser!.DeactivateAccount();
            dbContext.Entry(currentUser).State = EntityState.Modified;
            var result = await dbContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                return "User account deactivated successfully.";
            }

            return DomainUserErrors.UpdateFailed(
                "Sorry, account could be deactivated. Please again later."
            );
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                userId,
                "Account could not be deactivated"
            );
        }
    }

    public async ValueTask<ErrorOr<string>> ActivateAccountAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ActivateAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Get user
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser is null)
            {
                logger.LogError("Could not find user account: {UserId}", userId);
                OtelUserConstants.AddErrorEventById(
                    userId,
                    activity,
                    DomainErrors.NotFound("User")
                );
                return DomainErrors.NotFound("User");
            }

            // Activate account
            currentUser!.ActivateAccount();
            dbContext.Entry(currentUser).State = EntityState.Modified;
            var result = await dbContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                return "User account activated successfully.";
            }

            return DomainUserErrors.UpdateFailed(
                "Sorry, account could be activated. Please again later."
            );
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity,
                userId,
                "Account could not be activated"
            );
        }
    }

    private async ValueTask<ErrorOr<RefreshTokenResponse>> ValidateTokenAndPatchUser(
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
        var result = await userManager
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
            var error = DomainErrors.UpdateFailed(
                "Sorry, request failed, please try again.",
                "Token"
            );
            logger.LogToError(
                currentUser.UserName!,
                $"User: {currentUser.UserName} token patch failed."
            );
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

        var collections = userManager.Users.AsNoTracking().TagWith("FilterUsers").AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(pagingParameters.Search))
        {
            var searchPattern = $"%{pagingParameters.Search!.Trim()}%";
            collections = collections.Where(a =>
                EF.Functions.ILike(a.FullName, searchPattern)
                || EF.Functions.ILike(a.PhoneNumber!, searchPattern)
                || EF.Functions.ILike(a.Email!, searchPattern)
                || EF.Functions.ILike(a.UserName!, searchPattern)
            );
        }

        // Sort
        if (pagingParameters.OrderBy is null)
        {
            collections = collections.OrderBy(a => a.FullName);
        }

        return collections;
    }

    public async ValueTask<ErrorOr<int>> CountUsers(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.CreateActivity(nameof(CountUsers), ActivityKind.Server);
        activity?.Start();

        var count = await userManager.Users.CountAsync(cancellationToken);
        return count;
    }

    public async ValueTask<ErrorOr<int>> CountAdmins()
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(CountAdmins),
            ActivityKind.Server
        );
        activity?.Start();

        var admins = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
        return admins.Count;
    }

    // Logout user and save session
    public async ValueTask<ErrorOr<LogoutResponse>> LogoutUser(
        Guid sessionId,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(nameof(LogoutUser), ActivityKind.Server);
        activity?.Start();

        // Record logout info
        var logoutInfo = new LogoutResponse { LogoutAt = TimeProvider.System.GetUtcNow() };

        var result = await dbContext
            .UserSessions.Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(u => u.IsActive, false)
                        .SetProperty(u => u.LogoutAt, logoutInfo.LogoutAt)
                        .SetProperty(u => u.LastActivityAt, logoutInfo.LogoutAt)
                        .SetProperty(u => u.DateModified, logoutInfo.LogoutAt)
                        .SetProperty(u => u.ModifiedBy, tenantService.Name!),
                cancellationToken
            );

        if (result <= 0)
        {
            var error = DomainErrors.UpdateFailed("Could not logout user.", "User");
            logger.LogError("User session: {SessionId} logout failed.", sessionId);
            OtelUserConstants.AddErrorEventById(tenantService?.UserId!, activity, error);
            return error;
        }

        return logoutInfo;
    }
}
