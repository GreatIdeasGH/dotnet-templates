using GreatIdeas.Template.Application.Abstractions.Repositories;

namespace GreatIdeas.Template.Application.Features.Account.Login;

public interface IAccountLoginHandler : IApplicationHandler
{
    ValueTask<ErrorOr<LoginResponse>> LoginAccountHandler(
        LoginRequest request,
        CancellationToken cancellationToken
    );
}

internal sealed class AccountLoginHandler(
    IUserRepository userRepository,
    ILogger<AccountLoginHandler> logger
) : IAccountLoginHandler
{
    private static readonly ActivitySource _activitySource = new(nameof(AccountLoginHandler));

    public async ValueTask<ErrorOr<LoginResponse>> LoginAccountHandler(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var getUserActivity = _activitySource.CreateActivity(
            nameof(LoginAccountHandler),
            ActivityKind.Server
        );
        getUserActivity?.Start();

        // Get user
        try
        {
            var loginResponse = await userRepository.Login(request, cancellationToken);
            if (loginResponse.IsError)
            {
                // Add event
                OtelUserConstants.AddErrorEvent(
                    request.Username,
                    activity: getUserActivity,
                    loginResponse.FirstError
                );
                return loginResponse.Errors;
            }

            // Add event
            getUserActivity?.SetTag("user", request.Username);
            OtelUserConstants.AddInfoEvent(
                request.Username,
                activity: getUserActivity,
                message: "User logged in successfully"
            );
            return loginResponse.Value;
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCriticalUser(
                logger,
                activity: getUserActivity,
                user: request.Username,
                message: "Could not login user"
            );
        }
    }
}
