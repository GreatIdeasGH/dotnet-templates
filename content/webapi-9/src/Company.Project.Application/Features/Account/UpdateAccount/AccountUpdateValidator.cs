using RegexValidator = Company.Project.Application.Common.Extensions.RegexValidator;

namespace Company.Project.Application.Features.Account.UpdateAccount;

public sealed class AccountUpdateValidator : AbstractValidator<AccountUpdateRequest>
{
    public AccountUpdateValidator()
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

        RuleFor(x => x.Role).NotEmpty().WithMessage(errorMessage: "Select user role");
    }
}
