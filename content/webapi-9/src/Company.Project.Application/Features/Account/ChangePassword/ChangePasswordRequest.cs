namespace Company.Project.Application.Features.Account.ChangePassword;

public struct ChangePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
