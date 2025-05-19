namespace GreatIdeas.Template.Application.Features.Account.GetAccount;

public struct UserAccountResponse
{ 
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}


