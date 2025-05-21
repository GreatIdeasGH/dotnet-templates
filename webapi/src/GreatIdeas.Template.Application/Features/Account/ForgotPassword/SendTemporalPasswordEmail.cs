using GreatIdeas.Template.Application.Features.Account.ConfirmEmail;
using GreatIdeas.Template.Application.Responses.Authentication;

namespace GreatIdeas.Template.Application.Features.Account.ForgotPassword;

public sealed class SendTemporalPasswordEmail(
    IEmailSender emailSender,
    ILogger<SendConfirmationEmail> logger)
{
    public async Task ScheduleEmail(ForgottenPasswordResponse model)
    {
        await SendEmail(model);
        logger.LogToInfo($"Job ID: {model.Email}. Account confirmation has been enqueued.");
    }

    public async Task SendEmail(ForgottenPasswordResponse model)
    {
        // Get the web URL from httpContext
        var message = @"<h3>GreatIdeas.Template.WebAPI - Forgotten Password</h3>" +
                      "<p>Please use the following temporal password to login to your account. After login, change the password.</p>" +
                      "<p>Temporal Password: <strong>" + model.PasswordResetToken + "</strong></p>" +
                      "<br>" +
                      "<h4>" + EmailDetails.TeamNameTag + "</h4>" +
                      "<h3>" + EmailDetails.BusinessNameTag + "</h3>" +
                      "<p>Email: " + EmailDetails.FromAddress + "</p>" +
                      "<p>Website: " + EmailDetails.Website + "</p>";

        var response = await emailSender.SendEmailAsync(
            model.Email,
            "GreatIdeas.Template.WebAPI Temporal Password Reset",
            message,
            EmailDetails.SenderName
        );

        if (response)
        {
            logger.LogInformation("Email sent successfully");
        }
        else
        {
            logger.LogError("Email failed to send to {Email}", model.Email);
        }
    }
}