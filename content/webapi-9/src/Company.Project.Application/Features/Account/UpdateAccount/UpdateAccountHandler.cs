namespace Company.Project.Application.Features.Account.UpdateAccount;

public interface IUpdateAccountHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    );
}

internal sealed class UpdateAccountHandler(
    IUserRepository userRepository,
    ILogger<UpdateAccountHandler> logger,
    ExceptionNotificationService notificationService
) : IUpdateAccountHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(UpdateAccountHandler));

    public async ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(UpdateProfile),
            ActivityKind.Server
        );
        activity?.Start();

        // Get user
        try
        {
            var response = await userRepository.UpdateAccountAsync(
                userId,
                request,
                cancellationToken
            );
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
                message: "Could not update account"
            );
        }
    }
}
