namespace Company.Project.Application.Features.Account.ActivateAccount;

public interface IActivateAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> DeactivateAccount(
        string userId,
        CancellationToken cancellationToken
    );
}

internal sealed class ActivateAccountHandler(
    IUserRepository userRepository,
    ILogger<ActivateAccountHandler> logger,
    ExceptionNotificationService notificationService
) : IActivateAccountHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ActivateAccountHandler));

    public async ValueTask<ErrorOr<ApiResponse>> DeactivateAccount(
        string userId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(DeactivateAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // Get user
        try
        {
            var response = await userRepository.ActivateAccountAsync(userId, cancellationToken);
            if (response.IsError)
            {
                // Add event
                return response.Errors;
            }

            return new ApiResponse(Message: response.Value);
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCriticalUser(
                logger,
                notificationService,
                activity: activity,
                user: userId!,
                message: "Could not activate account"
            );
        }
    }
}
