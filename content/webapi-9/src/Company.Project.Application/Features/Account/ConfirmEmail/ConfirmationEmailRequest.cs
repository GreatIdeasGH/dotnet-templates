namespace Company.Project.Application.Features.Account.ConfirmEmail;

public sealed record ConfirmationEmailRequest(string Email, string UserId, string VerificationCode);
