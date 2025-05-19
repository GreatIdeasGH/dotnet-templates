namespace GreatIdeas.Template.Domain.Entities;

public sealed class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string AccountType { get; set; } = default!;

    [PersonalData]
    public bool IsActive { get; set; }

    [PersonalData]
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

    [PersonalData]
    public string? RefreshToken { get; set; }

    public void DeactivateAccount() => IsActive = false;

    public void ActivateAccount() => IsActive = true;

    public void Update(string phoneNumber, string username, bool isActive)
    {
        PhoneNumber = phoneNumber.Trim();
        UserName = username.Trim().ToLowerInvariant();
        NormalizedUserName = username.Trim().ToUpperInvariant();
        IsActive = isActive;
    }

    public void Update(string phoneNumber, string username, bool isActive, string email)
    {
        PhoneNumber = phoneNumber.Trim();
        UserName = username.Trim().ToLowerInvariant();
        NormalizedUserName = username.Trim().ToUpperInvariant();
        IsActive = isActive;
        Email = email.ToLowerInvariant();
        NormalizedEmail = email.Trim().ToUpperInvariant();
    }

    public void ApproveAccount()
    {
        IsActive = true;
        EmailConfirmed = true;
        PhoneNumberConfirmed = true;
    }

} //end ApplicationUser

//end namespace Entities
