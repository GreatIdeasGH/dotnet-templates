namespace Company.Project.Application.Responses.Authentication;

public record struct ForgottenPasswordResponse(
    string UserId,
    string Email,
    string FullName,
    string PasswordResetToken
);
