using System.Text;
using GreatIdeas.Template.Application.Features.Account;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Application.Features.Account.Login;
using GreatIdeas.Template.Application.Features.Account.Register;
using GreatIdeas.Template.Application.Features.Account.ResetPassword;
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
    private static readonly ActivitySource _activitySource = new(nameof(UserRepository));
    private readonly JwtService _jwtService = jwtService;
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async ValueTask<ApplicationUser?> FindById(string userId)
    {
        using var activity = _activitySource.CreateActivity(nameof(FindById), ActivityKind.Server);
        activity?.Start();

        return await _dbContext.Users.FindAsync(userId);
    }

    public async ValueTask<ErrorOr<UserAccountResponse>> GetUserAccountAsync(string userId)
    {
        using var activity = _activitySource.CreateActivity(
            nameof(GetUserAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var user = await _dbContext.Users.FindAsync(userId);
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

    public async ValueTask<IList<Claim>?> GetClaims(ApplicationUser user)
    {
        using var activity = _activitySource.CreateActivity(nameof(GetClaims), ActivityKind.Server);
        activity?.Start();

        return await userManager.GetClaimsAsync(user);
    }

    public async Task<ErrorOr<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity("GetUser", ActivityKind.Server);
        activity?.Start();
        try
        {
            var currentUser = await _dbContext
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
            using var passwordActivity = _activitySource.CreateActivity(
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
        using var activity = _activitySource.CreateActivity(
            nameof(CreateAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // start transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            // Create account
            // activity
            using var createUserActivity = _activitySource.CreateActivity(
                "CreateUser",
                ActivityKind.Server
            );

            // Create user
            var userExists = await _dbContext
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
                using var addClaimsActivity = _activitySource.CreateActivity(
                    "AddRoleClaims",
                    ActivityKind.Server
                );

                _ = await userManager.AddClaimsAsync(
                    userEntity,
                    [
                        new Claim(JwtClaimTypes.Id, $"{userEntity.Id}"),
                        new Claim(UserClaims.Username, userEntity.UserName!),
                        new Claim(UserClaims.AccountType, userEntity.AccountType!),
                    ]
                );

                // Add user role
                _ = await userManager.AddToRoleAsync(
                    userEntity,
                    AccountType.FromName(request.AccountType, true).Name
                );

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
                    Username: userEntity.UserName!,
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

    public async ValueTask<ErrorOr<SignUpResponse>> RegisterAccount(
        SignUpRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity(
            nameof(CreateAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // start transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            // Create account
            // activity
            using var createUserActivity = _activitySource.CreateActivity(
                "CreateUser",
                ActivityKind.Server
            );

            // Create user
            var userExists = await _dbContext
                .Users.AsNoTracking()
                .AnyAsync(x => x.PhoneNumber == request.PhoneNumber.Trim(), cancellationToken);
            if (userExists)
            {
                var userExistsMessage = DomainUserErrors.PhoneNumberExists(request.PhoneNumber!);
                OtelUserConstants.AddErrorEvent(request.PhoneNumber, activity, userExistsMessage);
                return userExistsMessage;
            }

            // Create user
            var userEntity = request.ToUser();
            var result = await userManager.CreateAsync(userEntity, request.Password);

            // create savepoint: create user
            await transaction.CreateSavepointAsync(
                "create user",
                cancellationToken: cancellationToken
            );

            if (result.Succeeded)
            {
                // Add claims
                using var addClaimsActivity = _activitySource.CreateActivity(
                    "AddRoleClaims",
                    ActivityKind.Server
                );

                _ = await userManager.AddClaimsAsync(
                    userEntity,
                    [
                        new(JwtClaimTypes.Id, $"{userEntity.Id}"),
                        new(JwtClaimTypes.Name, request.Name.Trim()),
                        new(UserClaims.Username, userEntity.UserName!),
                        new(UserClaims.AccountType, userEntity.AccountType!),
                    ]
                );

                // Add user role
                _ = await userManager.AddToRoleAsync(userEntity, AccountType.User.Name);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                logger.LogUserInfo(
                    userEntity.PhoneNumber!,
                    "User registered an account successfully."
                );

                return new SignUpResponse(
                    Name: request.GuardianName,
                    PhoneNumber: request.PhoneNumber,
                    Message: "Account registration has been submitted. You'll receive a message shortly."
                );
            }

            var error = DomainUserErrors.CreationFailed(
                result.Errors.FirstOrDefault()!.Description
            );
            logger.LogToError("username", error.Description);
            OtelUserConstants.AddErrorEvent(request.PhoneNumber, activity, error);
            return error;
        }
        catch (Exception exception)
        {
            // Rollback transaction
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            return exception.LogCriticalUser(
                logger,
                activity: activity,
                user: request.PhoneNumber!,
                message: "Account registration failed"
            );
        }
    }

    public ValueTask<ErrorOr<string>> UpdateProfileAsync(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    // Update user profile
    public async ValueTask<ErrorOr<string>> UpdateStudentProfile(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity(
            nameof(UpdateStudentProfile),
            ActivityKind.Server
        );
        activity?.Start();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        Error error;

        try
        {
            var query = _dbContext.Users.Where(x => x.Id == userId);
            var existingUser = await query.FirstOrDefaultAsync(cancellationToken);

            if (existingUser is null)
            {
                error = DomainUserErrors.UserNotFound;
                logger.LogUserError(userId, error.Description);
            }

            existingUser!.Update(request.PhoneNumber, request.Username, request.IsActive);
            _dbContext.Entry(existingUser).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                var claims = await userManager.GetClaimsAsync(existingUser!);

                // Update claims for username
                var claimsToDelete = claims.Where(x => x.Type == UserClaims.Username).ToList();
                if (claimsToDelete.Count > 0)
                {
                    var res = await userManager.RemoveClaimsAsync(existingUser!, claimsToDelete);
                    if (res.Succeeded)
                    {
                        // Update claims
                        var usernameClaim = new Claim(UserClaims.Username, request.Username.Trim());
                        await userManager.AddClaimsAsync(existingUser!, [usernameClaim]);
                    }
                }

                var message = "User profile updated successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEvent(userId, message, activity);

                await transaction.CommitAsync();
                return message;
            }

            error = DomainUserErrors.UpdateFailed("Could not update user profile");
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

    // Update user profile
    public async ValueTask<ErrorOr<string>> UpdateStaffAccountAsync(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity(
            nameof(UpdateStaffAccountAsync),
            ActivityKind.Server
        );
        activity?.Start();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingUser = await _dbContext
                .Users.Where(x => x.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            existingUser!.Update(
                request.PhoneNumber,
                request.Username,
                request.IsActive,
                request.Email
            );
            _dbContext.Entry(existingUser).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                var claims = await userManager.GetClaimsAsync(existingUser!);

                // Update claims for username
                var claimsToDelete = claims
                    .Where(x => x.Type == UserClaims.Username || x.Type == JwtClaimTypes.Email)
                    .ToList();
                if (claimsToDelete.Count > 0)
                {
                    var res = await userManager.RemoveClaimsAsync(existingUser!, claimsToDelete);
                    if (res.Succeeded)
                    {
                        // Update claims
                        var usernameClaim = new Claim(UserClaims.Username, request.Username.Trim());
                        var emailClaim = new Claim(JwtClaimTypes.Email, request.Email.Trim());
                        await userManager.AddClaimsAsync(
                            existingUser!,
                            [usernameClaim, emailClaim]
                        );
                    }
                }

                // Update role
                var roles = await userManager.GetRolesAsync(existingUser!);
                if (roles.Any())
                {
                    await userManager.RemoveFromRolesAsync(existingUser!, roles);
                    await userManager.AddToRoleAsync(existingUser!, request.Role);
                }

                var message = "Staff account updated successfully.";
                logger.LogUserInfo(userId, message);
                OtelUserConstants.AddInfoEvent(userId, message, activity);

                await transaction.CommitAsync();
                return message;
            }

            var error = DomainUserErrors.UpdateFailed("Could not update staff account");
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

    // Update student claims profile
    public async ValueTask<bool> UpdateStaffClaimsAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity(
            nameof(UpdateStaffClaimsAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var user = await _dbContext
            .Users.Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is not null)
        {
            var claims = await userManager.GetClaimsAsync(user!);

            // Update claims for username
            var claimsToDelete = claims.Where(x => x.Type == JwtClaimTypes.Name).ToList();
            if (claimsToDelete.Count > 0)
            {
                var res = await userManager.RemoveClaimsAsync(user!, claimsToDelete);
                if (res.Succeeded)
                {
                    // Update claims
                    var nameClaim = new Claim(JwtClaimTypes.Name, fullName.Trim());
                    await userManager.AddClaimAsync(user!, nameClaim);
                }
            }

            var message = "Updated name claim successfully.";
            logger.LogUserInfo(userId, message);
            OtelUserConstants.AddInfoEvent(userId, message, activity);
            return true;
        }

        var error = DomainUserErrors.UpdateFailed("Could not update user profile");
        logger.LogUserError(userId, error.Description);
        OtelUserConstants.AddErrorEvent(userId, activity, error);
        return false;
    }

    public async ValueTask<ErrorOr<string>> ResetPassword(
        string userId,
        PasswordResetRequest request
    )
    {
        // Start activity
        using var activity = _activitySource.CreateActivity(
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

    public async ValueTask AddClaim(string userId, string claimType, string claimValue)
    {
        using var activity = _activitySource.CreateActivity(nameof(AddClaim), ActivityKind.Server);
        activity?.Start();

        var user = await userManager.FindByIdAsync(userId);
        await userManager.AddClaimAsync(user!, new(claimType, claimValue));
    }

    public async ValueTask AddClaim(ApplicationUser user, string claimType, string claimValue)
    {
        using var activity = _activitySource.CreateActivity(nameof(AddClaim), ActivityKind.Server);
        activity?.Start();

        await userManager.AddClaimAsync(user, new(claimType, claimValue));
    }

    public async ValueTask RemoveClaim(ApplicationUser user, string claimType, string claimValue)
    {
        using var activity = _activitySource.CreateActivity(nameof(AddClaim), ActivityKind.Server);
        activity?.Start();

        await userManager.RemoveClaimAsync(user, new(claimType, claimValue));
    }

    // Update user claims
    public async ValueTask<bool> UpdateClaim(string userId, string claimType, string claimValue)
    {
        using var activity = _activitySource.CreateActivity(
            nameof(UpdateClaim),
            ActivityKind.Server
        );
        activity?.Start();

        var user = await _dbContext.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();

        if (user is not null)
        {
            var claims = await userManager.GetClaimsAsync(user!);

            // Update claims for username
            var claimsToDelete = claims.Where(x => x.Type == claimType).ToList();
            if (claimsToDelete.Count > 0)
            {
                var res = await userManager.RemoveClaimsAsync(user!, claimsToDelete);
                if (res.Succeeded)
                {
                    // Update claims
                    var newClaim = new Claim(claimType, claimValue.Trim());
                    await userManager.AddClaimAsync(user!, newClaim);
                }
            }

            var message = $"Updated {claimValue} claim successfully.";
            logger.LogUserInfo(userId, message);
            OtelUserConstants.AddInfoEvent(userId, message, activity);
            return true;
        }

        var error = DomainUserErrors.UpdateFailed("Could not update claim");
        logger.LogUserError(userId, error.Description);
        OtelUserConstants.AddErrorEvent(userId, activity, error);
        return false;
    }

    public async ValueTask<int> HasAdminRole(string userId)
    {
        using var activity = _activitySource.CreateActivity(
            nameof(HasAdminRole),
            ActivityKind.Server
        );
        activity?.Start();

        var users = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
        users = users.Where(x => x.Id == userId).ToList();
        return users.Count;
    }

    private async Task<ErrorOr<RefreshTokenResponse>> ValidateTokenAndPatchUser(
        ApplicationUser currentUser,
        CancellationToken cancellationToken
    )
    {
        using var tokenActivity = _activitySource.CreateActivity(
            "ValidateRefreshToken",
            ActivityKind.Server
        );
        tokenActivity?.Start();

        // Validate refresh token
        var tokenResponse = await _jwtService.ValidateRefreshToken(currentUser);

        // Update refresh token
        currentUser.RefreshToken = tokenResponse.RefreshToken;
        currentUser.RefreshTokenExpiryTime = tokenResponse.Expires;
        tokenActivity?.Stop();

        using var patchUserActivity = _activitySource.CreateActivity(
            "PatchLoginUser",
            ActivityKind.Server
        );
        patchUserActivity?.Start();
        var result = await _dbContext
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
