using System.ComponentModel.DataAnnotations;

namespace Company.Project.Application.Features.Account.CreateAccount;

public struct CreateAccountRequest
{
    [Required]
    public string FullName { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string Role { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string ConfirmPassword { get; set; }
}

public record AccountCreatedResponse(string UserId, string Email, string VerificationCode);
