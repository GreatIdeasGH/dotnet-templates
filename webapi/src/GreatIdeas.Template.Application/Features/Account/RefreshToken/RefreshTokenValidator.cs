namespace GreatIdeas.Template.Application.Features.Account.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token must be provided");

        // JWT must have three segments (JWS) or five segments (JWE).
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token must be provided")
            .Must(x => x?.Split('.').Length == 3 || x?.Split('.').Length == 5)
            .WithMessage("Access token must be a valid JWT");
    }
}