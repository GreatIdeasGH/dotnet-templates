using RegexValidator = GreatIdeas.Template.Application.Common.Extensions.RegexValidator;

namespace GreatIdeas.Template.Application.Features.Account.ResetPassword;

public sealed class PasswordResetValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(errorMessage: "New password is required")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("New password cannot contain space")
            .MinimumLength(6)
            .WithMessage("New password must be at least 6 characters");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage(errorMessage: "Confirm password is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Password and confirm password do not match")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
