namespace GreatIdeas.Template.Application.Features.Account.UpdateAccount;


public struct AccountUpdateRequest
{
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
