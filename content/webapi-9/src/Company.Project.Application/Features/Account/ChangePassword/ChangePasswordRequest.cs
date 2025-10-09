namespace Company.Project.Application.Features.Account.ChangePassword;

public sealed record ChangePasswordRequest(string OldPassword, string NewPassword);
