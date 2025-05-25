namespace GreatIdeas.Template.Application.Features.Account.ResendEmail;

public record struct ResendEmailRequest
{
    public string Email { get; set; }
}