using GreatIdeas.Template.Application.Abstractions.Repositories;
using GreatIdeas.Template.Application.Features.Account.UpdateAccount;

namespace GreatIdeas.Template.Application.Features.Account.UpdateProfile;

public interface IProfileUpdateHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        ProfileUpdateRequest request,
        CancellationToken cancellationToken
    );
}

internal sealed class ProfileUpdateHandler(
    IUserRepository userRepository,
    ILogger<ProfileUpdateHandler> logger
) : IProfileUpdateHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(ProfileUpdateHandler));

    public async ValueTask<ErrorOr<ApiResponse>> UpdateProfile(
        string userId,
        ProfileUpdateRequest request,
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
            var response = await userRepository.UpdateProfileAsync(
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
                message: "Could not update user profile"
            );
        }
    }
}