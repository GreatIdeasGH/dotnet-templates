using GreatIdeas.Template.Application.Common;
using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Features.Account.CreateAccount;

public sealed record AccountCreatedEvent(string Email) : EventBase;

public sealed class AccountCreatedConsumer(ILogger<AccountCreatedConsumer> logger)
    : IConsumer<AccountCreatedEvent>
{
    private static readonly ActivitySource ActivitySource = new(nameof(AccountCreatedConsumer));

    public Task Consume(ConsumeContext<AccountCreatedEvent> context)
    {
        using var createUserActivity = ActivitySource.CreateActivity(
            nameof(AccountCreatedConsumer),
            ActivityKind.Consumer
        );
        createUserActivity?.Start();

        // Schedule a message to be sent to the user's email address
        LogDefinitions.LogUserInfo(
            logger,
            context.Message.Email,
            "Consumed account created event. Sending message ..."
        );

        return Task.CompletedTask;
    }
}
