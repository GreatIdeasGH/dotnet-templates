namespace GreatIdeas.Template.Domain.Entities;

public sealed class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string FullName { get; set; } = null!;

    [PersonalData]
    public bool IsActive { get; set; }

    [PersonalData]
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

    [PersonalData]
    public string? RefreshToken { get; set; }

    public void DeactivateAccount() => IsActive = false;

    public void ActivateAccount() => IsActive = true;

    public void Update(string fullName, string phoneNumber)
    {
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
    }

} //end ApplicationUser

//end namespace Entities
