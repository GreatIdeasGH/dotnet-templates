using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Services;

internal sealed class EmailSender(ILogger<EmailSender> _logger)
    : IEmailSender
{
    private static readonly ActivitySource ActivitySource = new("EmailSender");

    public Task<bool> SendEmailAsync(string email, string subject, string body, string name)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(SendEmailAsync),
            ActivityKind.Server
        );
        activity?.Start();
        try
        {
           

            _logger.LogError("Email failed to send to {Email}", email);
            OtelUserConstants.AddErrorEvent(
                email,
                activity,
                Error.Failure(description: "Email failed to send")
            );
            return Task.FromResult(false);
        }
        catch (Exception exception)
        {
            var message = $"Email failed to send to {email}";
            OtelUserConstants.AddExceptionEvent(email, activity, exception);
            LogDefinitions.LogToCritical(_logger, exception, message);
            return Task.FromResult(false);
        }
    }
}
