namespace GreatIdeas.Template.Application.Features.Account.ResetPassword;

public struct PasswordResetRequest
{
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}
