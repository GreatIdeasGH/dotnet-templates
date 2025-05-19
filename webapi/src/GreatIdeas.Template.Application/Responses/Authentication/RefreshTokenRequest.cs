namespace GreatIdeas.Template.Application.Responses.Authentication;

public record struct RefreshTokenRequest(string Token, DateTime Expires);
