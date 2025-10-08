using Company.Project.Application.Common;
using Company.Project.Application.Responses.Authentication;

namespace Company.Project.Application.Features.Account.ForgotPassword;

public sealed record ForgottenPasswordEvent(ForgottenPasswordResponse Response) : EventBase;

public sealed class ForgottenPasswordConsumer(
    ILogger<ForgottenPasswordConsumer> logger,
    SendTemporalPasswordEmail sendTemporalPassword
) : IConsumer<ForgottenPasswordEvent>
{
    public Task Consume(ConsumeContext<ForgottenPasswordEvent> context)
    {
        // Schedule a message to be sent to the user's email address
        logger.LogInformation(
            "Scheduling temporal password for {UserId}",
            context.Message.Response.UserId
        );

        return sendTemporalPassword.ScheduleEmail(context.Message.Response);
    }
}
