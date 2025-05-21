namespace GreatIdeas.Template.Application.Features.Account.CreateAccount;

public sealed class AccountCreationValidator : AbstractValidator<CreateAccountRequest>
{
    public AccountCreationValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(errorMessage: "Full name is required")
            .MinimumLength(3);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(errorMessage: "Email address is required")
            .EmailAddress()
            .WithMessage("Email address is not valid");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(errorMessage: "Username is required")
            .EmailAddress()
            .WithMessage("Username should be a valid email address");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(errorMessage: "Phone number is required")
            .Matches(RegexValidator.PhoneNumberRegex())
            .WithMessage("Phone number is not valid")
            .Length(10)
            .WithMessage("Phone number should be 10 digits");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(errorMessage: "Password is required")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage(errorMessage: "Confirm password is required")
            .Equal(x => x.Password)
            .WithMessage("Password and confirm password do not match")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
