namespace Company.Project.Application.Features.Account.RefreshToken;

public record struct RefreshTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? Expires
);

public record struct RefreshTokenRequest(string AccessToken, string RefreshToken);
