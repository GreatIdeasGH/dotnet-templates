namespace Company.Project.Application.Features.Account.ResetPassword;

public interface IResetPasswordHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> UpdateProfile(string userId, PasswordResetRequest request);
}

internal sealed class ResetPasswordHandler(
    IUserRepository userRepository,
    ILogger<ResetPasswordHandler> logger,
    ExceptionNotificationService notificationService
) : IResetPasswordHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ResetPasswordHandler));

    public async ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        PasswordResetRequest request
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
            var response = await userRepository.ResetPassword(userId, request);
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
                message: "Could not update user profile"
            );
        }
    }
}
