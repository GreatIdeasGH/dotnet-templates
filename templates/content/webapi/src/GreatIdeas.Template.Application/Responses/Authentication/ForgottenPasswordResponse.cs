namespace GreatIdeas.Template.Application.Responses.Authentication;

public record struct ForgottenPasswordResponse(
    string UserId,
    string Email,
    string PasswordResetToken
);
