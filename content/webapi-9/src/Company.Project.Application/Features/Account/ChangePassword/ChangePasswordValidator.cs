namespace Company.Project.Application.Features.Account.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Old password is required");
        RuleFor(x => x.NewPassword)
            .Matches(RegexValidator.NoSpacesRegex())
            .WithMessage("Password cannot contain space");

        // Minimum 6 characters
        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");

        // Old password and new password cannot be the same
        RuleFor(x => x.OldPassword)
            .NotEqual(x => x.NewPassword)
            .WithMessage("Old password and new password cannot be the same");
    }
}
