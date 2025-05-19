namespace GreatIdeas.Template.Application.Responses.Authentication;

public record struct RefreshTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? Expires
);
