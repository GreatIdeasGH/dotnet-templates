using Microsoft.Extensions.Configuration;

namespace GreatIdeas.Template.Application.Features.Account.ConfirmEmail;

public sealed class SendConfirmationEmail(
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<SendConfirmationEmail> logger
)
{
    public async Task ScheduleEmail(ConfirmationEmailRequest model)
    {
        await SendEmail(model);
        logger.LogInformation("Job ID: {JobId}. Account confirmation has been enqueued.", model.Email);
    }

    public async Task SendEmail(ConfirmationEmailRequest model)
    {
        // Get the web URL from httpContext
        var webUrl = configuration["ApplicationSettings:WebUrl"];

        var callbackUrl =
            $"{webUrl}/account/confirmEmail?Id={model.UserId}&Code={model.VerificationCode}";
        var message =
            @"<h3>Congratulations! You have successfully created your account for the GreatIdeas.Template.WebAPI.</h3>"
            + "<p>Please click on the link below to confirm your email address.</p>"
            + "<p><a href=\""
            + callbackUrl
            + "\">Confirm your email</a></p>"
            + "<br>"
            + "<h4>"
            + EmailDetails.TeamNameTag
            + "</h4>"
            + "<h3>"
            + EmailDetails.BusinessNameTag
            + "</h3>"
            + "<p>Email: "
            + EmailDetails.FromAddress
            + "</p>"
            + "<p>Website: "
            + EmailDetails.Website
            + "</p>";

        var response = await emailSender.SendEmailAsync(
            model.Email,
            "GreatIdeas.Template.WebAPI Account Confirmation",
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