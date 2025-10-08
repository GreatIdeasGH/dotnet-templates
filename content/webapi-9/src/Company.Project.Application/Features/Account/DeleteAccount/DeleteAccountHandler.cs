using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Features.Account.DeleteAccount;

public interface IDeleteAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> DeleteAccount(
        string userId,
        CancellationToken cancellationToken
    );
}

internal sealed class DeleteAccountHandler(
    IUserRepository userRepository,
    ILogger<DeleteAccountHandler> logger,
    ExceptionNotificationService notificationService
) : IDeleteAccountHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(DeleteAccountHandler));

    public async ValueTask<ErrorOr<ApiResponse>> DeleteAccount(
        string userId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // Delete user
        try
        {
            var response = await userRepository.DeleteAccountAsync(userId, cancellationToken);
            if (response.IsError)
            {
                // Add event
                return DomainUserErrors.DeleteFailed("Account deletion failed");
            }

            return new ApiResponse("Account deleted successfully");
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCriticalUser(
                logger,
                notificationService,
                activity: activity,
                user: userId!,
                message: "Could not delete account"
            );
        }
    }
}
