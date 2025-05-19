namespace GreatIdeas.Template.Application.Features.Account.UpdateProfile;

public struct ProfileUpdateRequest
{
    public string PhoneNumber { get; set; }
    public string Username { get; set; }
    public bool IsActive { get; set; }
}

public struct AccountUpdateRequest
{
    public string Username { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
