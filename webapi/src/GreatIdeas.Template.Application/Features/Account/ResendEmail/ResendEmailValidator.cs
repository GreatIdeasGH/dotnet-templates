namespace GreatIdeas.Template.Application.Features.Account.ResendEmail;

public sealed class ResendEmailValidator : AbstractValidator<ResendEmailRequest>
{
    public ResendEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Please provide a valid email address");
    }
}