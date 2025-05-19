namespace GreatIdeas.Template.Application.Features.Account.Register;

public struct CreateAccountRequest
{
    public string Username { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; }
    public string AccountType { get; set; }

    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public struct SignUpRequest
{
    public string Name { get; set; }
    public string Username { get; set; }
    public DateTimeOffset BirthDate { get; set; }
    public string Gender { get; set; }
    public string GuardianName { get; set; }
    public string GuardianRelationship { get; set; }
    public string PhoneNumber { get; set; }
    public string Province { get; set; }
    public string Grade { get; set; }
    public string City { get; set; }
    public string Password { get; set; }
}

public record AccountCreatedResponse(string Username, string Email, string VerificationCode);

public record SignUpResponse(string Name, string PhoneNumber, string Message);
