using System.ComponentModel.DataAnnotations;

namespace Company.Project.Application.Features.Account.ForgotPassword;

public sealed record ForgotPasswordRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
}
