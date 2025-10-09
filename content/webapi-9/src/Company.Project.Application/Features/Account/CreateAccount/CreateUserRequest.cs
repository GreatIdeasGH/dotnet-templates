using System.ComponentModel.DataAnnotations;

namespace Company.Project.Application.Features.Account.CreateAccount;

public record struct CreateAccountRequest(
    string FullName,
    string Username,
    string Email,
    string PhoneNumber,
    string Role,
    string Password,
    string ConfirmPassword
);

public record AccountCreatedResponse(string UserId, string Email, string VerificationCode);
