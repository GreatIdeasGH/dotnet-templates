namespace Company.Project.Application.Features.Account.ActivateAccount;

public interface IDeactivateAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> DeactivateAccount(
        string userId,
        CancellationToken cancellationToken
    );
}

internal sealed class DeactivateAccountHandler(
    IUserRepository userRepository,
    ILogger<DeactivateAccountHandler> logger
) : IDeactivateAccountHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(DeactivateAccountHandler));

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
            var response = await userRepository.DeactivateAccountAsync(userId, cancellationToken);
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
                activity: activity,
                user: userId!,
                message: "Could not deactivate account"
            );
        }
    }
}
