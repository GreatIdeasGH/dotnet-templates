namespace GreatIdeas.Template.Application.Responses.Authentication;

public record AuthenticationResponse(string UserId, string Email, string FullName, string Token);
