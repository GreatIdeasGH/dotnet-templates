using GreatIdeas.Template.Application.Common.Errors;

namespace GreatIdeas.Template.Application.Features.Account.DeleteAccount;

public interface IDeleteAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> DeleteAccount(
        string userId,
        CancellationToken cancellationToken
    );
}

internal sealed class DeleteAccountHandler(
    IUserRepository userRepository,
    ILogger<DeleteAccountHandler> logger
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
                activity,
                userId!,
                "Could not delete account"
            );
        }
    }
}