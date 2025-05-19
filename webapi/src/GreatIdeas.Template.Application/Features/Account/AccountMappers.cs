using GreatIdeas.Template.Application.Features.Account.Register;
using GreatIdeas.Template.Domain.Entities;
using GreatIdeas.Template.Domain.Enums;

namespace GreatIdeas.Template.Application.Features.Account;

public static class AccountMappers
{
    public static ApplicationUser ToUser(this CreateAccountRequest request)
    {
        return new ApplicationUser
        {
            Email =
                request.Email?.Trim() ?? EmailDetails.GenerateTempEmail(request.Username.Trim()),
            UserName = request.Username.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            AccountType = AccountType.FromName(request.AccountType, true).Name
        };
    }

    public static ApplicationUser ToUser(this SignUpRequest request)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = EmailDetails.GenerateTempEmail(request.PhoneNumber.Trim()),
            UserName = request.Username?.Trim(),
            IsActive = false,
            EmailConfirmed = false,
            AccountType = AccountType.User.Name,
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhoneNumberConfirmed = false
        };
    }

}
