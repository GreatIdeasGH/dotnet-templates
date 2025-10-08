using RegexValidator = Company.Project.Application.Common.Extensions.RegexValidator;

namespace Company.Project.Application.Features.Account.UpdateProfile;

public sealed class ProfileUpdateValidator : AbstractValidator<ProfileUpdateRequest>
{
    public ProfileUpdateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(errorMessage: "Full name is required")
            .MinimumLength(3);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(errorMessage: "Phone number is required")
            .Matches(RegexValidator.PhoneNumberRegex())
            .WithMessage("Phone number is not valid")
            .Length(10)
            .WithMessage("Phone number should be 10 digits");
    }
}
