using GreatIdeas.Template.Application.Abstractions.Repositories;

namespace GreatIdeas.Template.Application.Features.Account.CreateAccount;

public interface IAccountCreationHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> RegisterAccountHandler(
        CreateAccountRequest request,
        CancellationToken cancellationToken
    );
}

internal sealed class AccountCreationHandler(
    IUserRepository userRepository,
    ILogger<AccountCreationHandler> logger,
    IPublishEndpoint publishEndpoint
) : IAccountCreationHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(AccountCreationHandler));

    public async ValueTask<ErrorOr<ApiResponse>> RegisterAccountHandler(
        CreateAccountRequest request,
        CancellationToken cancellationToken
    )
    {
        // Start activity
        using var createUserActivity = ActivitySource.CreateActivity(
            nameof(RegisterAccountHandler),
            ActivityKind.Server
        );
        createUserActivity?.Start();

        try
        {
            var response = await userRepository.CreateAccount(request, cancellationToken);
            if (response.IsError)
            {
                return response.Errors;
            }

            // publish event
            using var activity = ActivitySource.StartActivity(
                nameof(AccountCreatedEvent),
                ActivityKind.Producer
            );
            await publishEndpoint.Publish(
                new AccountCreatedEvent(response.Value.Email),
                cancellationToken
            );

            var message = $"User account created for {request.Username}";
            OtelConstants.AddSuccessEvent(message, activity);
            return new ApiResponse(Message: message);
        }
        catch (Exception exception)
        {
            return exception.LogCriticalUser(
                logger,
                activity: createUserActivity,
                user: request.PhoneNumber,
                message: "Could not register user account"
            );
        }
    }
}
