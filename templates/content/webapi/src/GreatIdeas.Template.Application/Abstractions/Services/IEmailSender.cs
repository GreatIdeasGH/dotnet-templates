namespace GreatIdeas.Template.Application.Abstractions.Services;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string email, string subject, string body, string name);
}
