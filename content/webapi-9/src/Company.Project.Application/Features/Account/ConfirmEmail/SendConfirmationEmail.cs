using Company.Project.Application.Common.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Company.Project.Application.Features.Account.ConfirmEmail;

public sealed class SendConfirmationEmail(
    IEmailSender emailSender,
    IConfiguration configuration,
    IOptionsMonitor<ApplicationSettings> applicationSettings,
    ILogger<SendConfirmationEmail> logger
)
{
    public async Task ScheduleEmail(ConfirmationEmailRequest model)
    {
        await SendEmail(model);
        logger.LogInformation(
            "Job ID: {JobId}. Account confirmation has been enqueued.",
            model.Email
        );
    }

    public async Task SendEmail(ConfirmationEmailRequest model)
    {
        var emailSettings = applicationSettings.CurrentValue.EmailSettings;
        // Get the web URL from httpContext
        var webUrl = configuration["ApplicationSettings:WebUrl"];

        var callbackUrl =
            $"{webUrl}/account/confirmEmail?Id={model.UserId}&Code={model.VerificationCode}";
        var message =
            @"<h3>Congratulations! You have successfully created your account for the GreatIdeas.WebAPI.</h3>"
            + "<p>Please click on the link below to confirm your email address.</p>"
            + "<p><a href=\""
            + callbackUrl
            + "\">Confirm your email</a></p>"
            + "<br>"
            + "<h4>"
            + emailSettings.TeamNameTag
            + "</h4>"
            + "<h3>"
            + emailSettings.BusinessNameTag
            + "</h3>"
            + "<p>Email: "
            + emailSettings.FromAddress
            + "</p>"
            + "<p>Website: "
            + emailSettings.Website
            + "</p>";

        var response = await emailSender.SendEmailAsync(
            email: model.Email,
            subject: $"{emailSettings.BusinessNameTag} Account Confirmation",
            body: message,
            name: emailSettings.FromName,
            useCc: false
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
