using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Features.Account.GetAccount;

public interface IGetUserAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<UserAccountResponse>> GetUserAccount(
        string userId,
        CancellationToken cancellationToken
    );
}

internal sealed class GetUserAccountHandler(
    IUserRepository userRepository,
    ILogger<GetUserAccountHandler> logger
) : IGetUserAccountHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(GetUserAccountHandler));

    public async ValueTask<ErrorOr<UserAccountResponse>> GetUserAccount(
        string userId,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(GetUserAccount),
            ActivityKind.Server
        );
        activity?.Start();

        // Get user
        try
        {
            var response = await userRepository.GetUserAccountAsync(userId, cancellationToken);
            if (response.IsError)
            {
                // Add event
                return DomainUserErrors.UserNotFound;
            }

            return response;
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCriticalUser(
                logger,
                activity,
                userId!,
                "Could not fetch user account"
            );
        }
    }
}
