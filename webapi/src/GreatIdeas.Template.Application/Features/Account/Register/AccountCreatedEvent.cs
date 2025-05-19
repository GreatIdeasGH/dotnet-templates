using GreatIdeas.Template.Application.Common;
using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Features.Account.Register;

public sealed record AccountCreatedEvent(string Username, string Email) : EventBase;

public sealed record AccountRegisteredEvent(string PhoneNumber, string Message) : EventBase;

public sealed class AccountCreatedConsumer(ILogger<AccountCreatedConsumer> logger)
    : IConsumer<AccountRegisteredEvent>
{
    private static readonly ActivitySource ActivitySource = new(nameof(AccountCreatedConsumer));

    public Task Consume(ConsumeContext<AccountRegisteredEvent> context)
    {
        using var createUserActivity = ActivitySource.CreateActivity(
            nameof(AccountCreatedConsumer),
            ActivityKind.Consumer
        );
        createUserActivity?.Start();

        // Schedule a message to be sent to the user's email address
        LogDefinitions.LogUserInfo(logger, context.Message.PhoneNumber,
            "Consumed account created event. Sending message ..."
        );
        
        return Task.CompletedTask;
    }
}
