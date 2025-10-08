using Company.Project.Application.Common;
using Company.Project.Application.Features.Account.ConfirmEmail;

namespace Company.Project.Application.Features.Account.ResendEmail;

public sealed record ResendEmailEvent(string UserId, string Email, string VerificationCode)
    : EventBase;

public sealed class ResendEmailConsumer(
    SendConfirmationEmail emailConfirmation,
    ILogger<ResendEmailConsumer> logger
) : IConsumer<ResendEmailEvent>
{
    public Task Consume(ConsumeContext<ResendEmailEvent> context)
    {
        logger.Created(Guid.Parse(context.Message.UserId), "User");

        // Schedule a message to be sent to the user's email address
        return emailConfirmation.ScheduleEmail(
            new ConfirmationEmailRequest(
                UserId: context.Message.UserId,
                Email: context.Message.Email,
                VerificationCode: context.Message.VerificationCode
            )
        );
    }
}
