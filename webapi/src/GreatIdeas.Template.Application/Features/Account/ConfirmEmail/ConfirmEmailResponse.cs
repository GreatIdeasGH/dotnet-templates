namespace GreatIdeas.Template.Application.Features.Account.ConfirmEmail;

public sealed record ConfirmEmailResponse
{
    public required string UserId { get; set; }
    public required string ConfirmationCode { get; set; }
}