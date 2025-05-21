namespace GreatIdeas.Template.Application.Features.Account.RefreshToken;

public interface IRefreshTokenHandler : IApplicationHandler
{
    ValueTask<ErrorOr<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request,
        CancellationToken cancellationToken);
}

internal sealed class RefreshTokenHandler(
    IUserRepository userRepository,
    ILogger<RefreshTokenHandler> logger) : IRefreshTokenHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(RefreshTokenHandler));

    public async ValueTask<ErrorOr<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // Start activity
        using var getUserActivity = ActivitySource.CreateActivity(nameof(RefreshToken), ActivityKind.Server);
        getUserActivity?.Start();

        // Get user
        try
        {
            var response = await userRepository.RefreshToken(request, cancellationToken);
            if (response.IsError)
            {
                return response.Errors;
            }

            return response.Value;
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(logger,
                getUserActivity,
                "Could not login user",
                "User");
        }
    }
}