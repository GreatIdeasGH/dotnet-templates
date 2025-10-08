using System.Web;
using Company.Project.Application.Common;
using Company.Project.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace Company.Project.Application.Services;

public sealed class ExceptionNotificationService(
    IEmailSender emailSender,
    ILogger<ExceptionNotificationService> logger,
    IOptionsMonitor<ApplicationSettings> optionsMonitor
)
{
    private static readonly ActivitySource ActivitySource = new(
        nameof(ExceptionNotificationService)
    );

    public async ValueTask NotifyAdminWithExceptionLog(
        Exception exception,
        ExceptionNotifications subject = ExceptionNotifications.UrgentBugNotification
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(NotifyAdminWithExceptionLog),
            ActivityKind.Server
        );
        activity?.Start();
        var emailSettings = optionsMonitor.CurrentValue.EmailSettings;

        var htmlContent = $"""
                <h2 style='color:red;'>Exception Notification for {emailSettings.BusinessNameTag}🐞</h2>
                <p><strong>Date:</strong> {TimeProvider.System.GetLocalNow()}</p>
                <p><strong>Platform:</strong> {Environment.OSVersion.Platform}</p>
                <p><strong>Environment:</strong> {Environment.GetEnvironmentVariable(
                    "ASPNETCORE_ENVIRONMENT"
                ) ?? "Production"}</p>
                <p><strong>OS:</strong> {Environment.OSVersion}</p>
                <p><strong>Machine Name:</strong> {Environment.MachineName}</p>
                <p><strong>Error Message:</strong></p>
                <code>{HttpUtility.HtmlEncode(exception.Message)}</code>
                <p><strong>Detailed Message:</strong></p>
                <code>{HttpUtility.HtmlEncode(exception.InnerException?.Message ?? "No Inner Exception")}</code>
                <p>Please take the necessary actions to resolve the following issue.</p>
                <footer>
                    <p>Automated email,<br/>Company.Project Dev Team</p>
                </footer>
                """;

        var toAddress = "support@domain.com";

        try
        {
            await emailSender.SendEmailAsync(
                logicAppUrl: emailSettings.LogicAppUrl,
                subject: $"{subject}: {TimeProvider.System.GetLocalNow()}",
                body: htmlContent,
                name: "Company.Project Dev Team",
                to: toAddress,
                from: $"{emailSettings.FromName} <{emailSettings.FromAddress}>",
                cc: "admin@domain.com",
                cancellationToken: default
            );
            logger.LogToInfo($"Exception email sent successfully to {toAddress}");
        }
        catch (Exception ex)
        {
            /// OperationID is contained in the exception message and can be used for troubleshooting purposes
            logger.LogToCritical(ex, $"Email send operation failed with error code: {ex.Message}");
        }
    }
}

public sealed record ExceptionNotificationEvent(
    Exception Exception,
    ExceptionNotifications Notification
) : EventBase;

public sealed class ExceptionNotificationConsumer(ExceptionNotificationService notificationService)
    : IConsumer<ExceptionNotificationEvent>
{
    private static readonly ActivitySource ActivitySource = new(
        nameof(ExceptionNotificationConsumer)
    );

    public async Task Consume(ConsumeContext<ExceptionNotificationEvent> context)
    {
        using var createUserActivity = ActivitySource.CreateActivity(
            nameof(Consume),
            ActivityKind.Consumer
        );
        createUserActivity?.Start();

        await notificationService.NotifyAdminWithExceptionLog(
            context.Message.Exception,
            context.Message.Notification
        );
    }
}
