using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Features.Account.Logout;

public interface IAccountLogoutHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse<LogoutResponse>>> LogoutAccountHandler(
        Guid sessionId,
        CancellationToken cancellationToken
    );
}

internal sealed class AccountLogoutHandler(
    IUserRepository userRepository,
    ILogger<AccountLogoutHandler> logger,
    ITenantService tenantService,
    ExceptionNotificationService notificationService
) : IAccountLogoutHandler
{
    private static readonly ActivitySource _activitySource = new(nameof(AccountLogoutHandler));

    public async ValueTask<ErrorOr<ApiResponse<LogoutResponse>>> LogoutAccountHandler(
        Guid sessionId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var getUserActivity = _activitySource.CreateActivity(
            nameof(LogoutAccountHandler),
            ActivityKind.Server
        );
        getUserActivity?.Start();

        // Get user
        try
        {
            var userId = tenantService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return DomainErrors.NotFound("User");
            }

            var logoutResponse = await userRepository.LogoutUser(sessionId, cancellationToken);
            if (logoutResponse.IsError)
            {
                // Add event
                OtelUserConstants.AddErrorEvent(userId, getUserActivity, logoutResponse.FirstError);
                return logoutResponse.Errors;
            }

            // Add event
            var message = "User logged out successfully";
            getUserActivity?.SetTag("user", userId);
            OtelUserConstants.AddInfoEventWithEmail(
                userId,
                activity: getUserActivity,
                message: message
            );
            return new ApiResponse<LogoutResponse>
            {
                Item = logoutResponse.Value,
                Message = message,
            };
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                notificationService,
                activity: getUserActivity,
                message: "Could not logout user",
                entityName: "User"
            );
        }
    }
}
