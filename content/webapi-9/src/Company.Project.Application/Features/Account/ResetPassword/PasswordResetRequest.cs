namespace Company.Project.Application.Features.Account.ResetPassword;

public sealed record PasswordResetRequest(string NewPassword, string ConfirmNewPassword);
