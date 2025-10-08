using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using Company.Project.Application.Common.Options;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

using Serilog;

namespace Company.Project.Application.Services;

internal sealed class EmailSender(
    ILogger<EmailSender> logger,
    IOptionsMonitor<ApplicationSettings> optionsMonitor
) : IEmailSender
{
    private static readonly ActivitySource ActivitySource = new(nameof(EmailSender));

    public async Task<bool> SendEmailAsync(
        string email,
        string subject,
        string body,
        string name,
        bool useCc
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(SendEmailAsync),
            ActivityKind.Server
        );
        activity?.Start();
        var emailSettings = optionsMonitor.CurrentValue.EmailSettings;

        var maxRetries = emailSettings.MaxRetryAttempts;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var success = await TrySendEmailAsync(
                    emailSettings,
                    email,
                    subject,
                    body,
                    name,
                    useCc
                );
                if (success)
                {
                    if (attempt > 1)
                    {
                        logger.LogInformation(
                            "Email sent successfully to {Email} with subject '{Subject}' on attempt {Attempt}/{MaxRetries}",
                            email,
                            subject,
                            attempt,
                            maxRetries
                        );
                    }
                    else
                    {
                        logger.LogInformation(
                            "Email sent successfully to {Email} with subject '{Subject}'",
                            email,
                            subject
                        );
                    }
                    return true;
                }
            }
            catch (Exception exception)
                when (IsTransientException(exception) && attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)
                );
                logger.LogWarning(
                    exception,
                    "Attempt {Attempt}/{MaxRetries} failed to send email to {Email}. Retrying in {Delay}ms. Error: {Error}",
                    attempt,
                    maxRetries,
                    email,
                    delay.TotalMilliseconds,
                    exception.Message
                );

                await Task.Delay(delay);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    $"Failed to send email to {email} with subject '{subject}' after {attempt} attempt(s)";
                logger.LogToCritical(exception, errorMessage);
                return false;
            }
        }

        var finalErrorMessage =
            $"Failed to send email to {email} with subject '{subject}' after {maxRetries} attempts";
        logger.LogToCritical(null, finalErrorMessage);
        return false;
    }

    public async Task<bool> SendEmailAsync(
        string logicAppUrl,
        string subject,
        string body,
        string name,
        string to,
        string from,
        string cc,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(SendEmailAsync),
            ActivityKind.Client
        );
        activity?.Start();

        using var httpClient = new HttpClient();

        var payload = new
        {
            Subject = subject,
            Body = body,
            Name = name,
            To = $"{name} <{to}>",
            From = from,
            Cc = cc,
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(logicAppUrl, content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            Log.Information("Email sent successfully via Logic App to {To}", to);
            return true;
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error(
                "Failed to send email via Logic App to {EmailTo} with response: {ResponseBody}",
                to,
                responseBody
            );
            return false;
        }
    }

    private async Task<bool> TrySendEmailAsync(
        EmailSettings emailSettings,
        string email,
        string subject,
        string body,
        string name,
        bool useCc
    )
    {
        var message = new MimeMessage();

        // Set sender
        message.From.Add(new MailboxAddress(emailSettings.FromName, emailSettings.FromAddress));

        // Set recipient
        message.To.Add(new MailboxAddress(name, email));

        // Set Cc
        if (!string.IsNullOrEmpty(emailSettings.CcAddress) && useCc)
        {
            message.Cc.Add(new MailboxAddress(emailSettings.CcName, emailSettings.CcAddress));
        }

        // Set subject
        message.Subject = subject;

        // Set body
        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        // SMTP configuration
        var smtpHost = emailSettings.SmtpHost;
        var smtpPort = int.Parse(emailSettings.SmtpPort);
        var smtpUser = emailSettings.SmtpUser;
        var smtpPassword = emailSettings.SmtpPassword;
        var useAuthentication =
            !string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPassword);
        var enableSsl = useAuthentication;

        // Create timeout cancellation token
        // using var timeoutCts = new CancellationTokenSource(
        //     TimeSpan.FromSeconds(emailSettings.TimeoutInSeconds)
        // )
        // var cancellationToken = timeoutCts.Token

        using var client = new SmtpClient();

        // Set timeout for the underlying socket operations
        client.Timeout = (int)
            TimeSpan.FromSeconds(emailSettings.TimeoutInSeconds).TotalMilliseconds;

        try
        {
            // Connect to SMTP server with timeout
            await client.ConnectAsync(
                smtpHost,
                smtpPort,
                enableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None
            );

            // Authenticate if credentials are provided
            if (useAuthentication || !smtpHost.Contains("localhost"))
            {
                await client.AuthenticateAsync(smtpUser, smtpPassword);
            }

            // Send the message with timeout
            await client.SendAsync(message);

            // Disconnect with timeout
            await client.DisconnectAsync(true);

            return true;
        }
        finally
        {
            // Ensure client is properly disposed even if operations were cancelled
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(quit: false);
                }
                catch
                {
                    // Ignore exceptions during cleanup
                }
            }
        }
    }

    private static bool IsTransientException(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            TaskCanceledException => true,
            OperationCanceledException => true,
            SocketException => true,
            IOException => true,
            HttpRequestException => true,
            _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) =>
                true,
            _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) =>
                true,
            _ when exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase) =>
                true,
            _ => false,
        };
    }
}
