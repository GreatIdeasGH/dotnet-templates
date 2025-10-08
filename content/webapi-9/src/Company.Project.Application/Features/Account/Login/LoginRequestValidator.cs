using RegexValidator = Company.Project.Application.Common.Extensions.RegexValidator;

namespace Company.Project.Application.Features.Account.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space");
    }
}
