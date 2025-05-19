using RegexValidator = GreatIdeas.Template.Application.Common.Extensions.RegexValidator;

namespace GreatIdeas.Template.Application.Features.Account.UpdateProfile;

public sealed class ProfileUpdateValidator : AbstractValidator<ProfileUpdateRequest>
{
    public ProfileUpdateValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(errorMessage: "Username is required")
            .Matches(RegexValidator.UsernameRegex())
            .WithMessage("Username should not contain spaces nor specials characters")
            .MinimumLength(3);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(errorMessage: "Phone number is required")
            .Matches(RegexValidator.PhoneNumberRegex())
            .WithMessage("Phone number is not valid")
            .Length(11)
            .WithMessage("Phone number should be 11 digits");
    }
}

public sealed class StaffAccountUpdateValidator : AbstractValidator<AccountUpdateRequest>
{
    public StaffAccountUpdateValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(errorMessage: "Email address is required")
            .EmailAddress()
            .WithMessage("Email address is not valid");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(errorMessage: "Username is required")
            .Matches(RegexValidator.UsernameRegex())
            .WithMessage("Username should not contain spaces nor specials characters")
            .MinimumLength(3);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(errorMessage: "Phone number is required")
            .Matches(RegexValidator.PhoneNumberRegex())
            .WithMessage("Phone number is not valid")
            .Length(11)
            .WithMessage("Phone number should be 11 digits");
    }
}
