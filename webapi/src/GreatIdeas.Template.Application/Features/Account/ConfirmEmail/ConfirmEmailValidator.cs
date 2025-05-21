namespace GreatIdeas.Template.Application.Features.Account.ConfirmEmail;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailResponse>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User Id is required");

        RuleFor(x => x.ConfirmationCode)
            .NotEmpty()
            .WithMessage("Confirmation code is required");
    }
}