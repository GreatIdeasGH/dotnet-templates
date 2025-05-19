using RegexValidator = GreatIdeas.Template.Application.Common.Extensions.RegexValidator;

namespace GreatIdeas.Template.Application.Features.Account.Register;

public sealed class AccountCreationValidator : AbstractValidator<CreateAccountRequest>
{
    public AccountCreationValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(15)
            .Matches(RegexValidator.UsernameRegex())
            .WithMessage("Username should not contain spaces nor specials characters")
            .Length(3, 15)
            .WithMessage("Username should be between 5 and 15 characters");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(errorMessage: "Full name is required")
            .Length(3, 50)
            .WithMessage("Full name should be between 3 and 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email is not valid")
            .Unless(x => string.IsNullOrWhiteSpace(x.Email));

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

public sealed class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(RegexValidator.PhoneNumberRegex())
            .WithMessage("Phone number must be 11 digits");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birth date is required")
            .Must(birthDate =>
                new DateTime(
                    birthDate.Year,
                    birthDate.Month,
                    birthDate.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                ) <= DateTime.Now.AddYears(-2)
            )
            .WithMessage("Birth date must be at least 2 years ago");

        RuleFor(x => x.BirthDate).NotEmpty().WithMessage("Birth date is required");

        RuleFor(x => x.Name).NotEmpty().WithMessage("Student name is required");

        RuleFor(expression: x => x.Gender).NotEmpty().WithMessage("Gender is required");

        RuleFor(x => x.Grade).NotEmpty().WithMessage("Grade is required");

        RuleFor(x => x.GuardianName).NotEmpty().WithMessage("Guardian name is required");

        RuleFor(x => x.GuardianRelationship)
            .NotEmpty()
            .WithMessage("Guardian relationship is required");

        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");

        RuleFor(x => x.Province).NotEmpty().WithMessage("Province is required");

        RuleFor(x => x.Username.Trim())
            .NotEmpty()
            .WithMessage(errorMessage: "Username is required")
            .Matches(RegexValidator.UsernameRegex())
            .WithMessage("Username should not contain spaces nor specials characters")
            .MinimumLength(3);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(errorMessage: "Password is required")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
