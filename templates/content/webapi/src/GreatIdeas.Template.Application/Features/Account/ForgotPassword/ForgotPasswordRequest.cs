namespace GreatIdeas.Template.Application.Features.Account.ForgotPassword;

public sealed record ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
}