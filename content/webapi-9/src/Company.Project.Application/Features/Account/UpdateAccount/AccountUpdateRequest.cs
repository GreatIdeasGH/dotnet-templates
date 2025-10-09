namespace Company.Project.Application.Features.Account.UpdateAccount;

public sealed record AccountUpdateRequest(
    string FullName,
    string Username,
    string Role,
    string Email,
    string PhoneNumber
);
