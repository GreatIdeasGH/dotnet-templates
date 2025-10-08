namespace Company.Project.Application.Abstractions.Services;

public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously using SMTP with default configuration.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="name">The recipient's display name.</param>
    /// <param name="useCc">Indicates whether to include CC recipients from default configuration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the email was sent successfully.</returns>
    Task<bool> SendEmailAsync(string email, string subject, string body, string name, bool useCc);

    /// <summary>
    /// Sends an email asynchronously through an Azure Logic App endpoint with full control over email parameters.
    /// </summary>
    /// <param name="logicAppUrl">The URL of the Logic App endpoint to use for sending the email.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="name">The recipient's display name.</param>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="from">The sender's email address.</param>
    /// <param name="cc">The CC recipients' email addresses (can be empty or null).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the email was sent successfully.</returns>
    Task<bool> SendEmailAsync(
        string logicAppUrl,
        string subject,
        string body,
        string name,
        string to,
        string from,
        string cc,
        CancellationToken cancellationToken = default
    );
}
