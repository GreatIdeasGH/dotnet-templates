using GreatIdeas.Template.Application.Abstractions.Repositories;

namespace GreatIdeas.Template.Application.Features.Account.UpdateProfile;

public interface IStaffAccountUpdateHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        AccountUpdateRequest request,
        CancellationToken cancellationToken
    );
}

internal sealed class StaffAccountUpdateHandler(
    IUserRepository userRepository,
    ILogger<StaffAccountUpdateHandler> logger
) : IStaffAccountUpdateHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(StaffAccountUpdateHandler));

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
            var response = await userRepository.UpdateStaffAccountAsync(
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
                activity: activity,
                user: userId!,
                message: "Could not update staff account"
            );
        }
    }
}
