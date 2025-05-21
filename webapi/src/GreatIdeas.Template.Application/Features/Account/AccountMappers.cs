using GreatIdeas.Template.Application.Features.Account.CreateAccount;
using GreatIdeas.Template.Application.Features.Account.GetAccount;
using GreatIdeas.Template.Domain.Entities;

namespace GreatIdeas.Template.Application.Features.Account;

public static class AccountMappers
{
    public static ApplicationUser ToUser(this CreateAccountRequest request)
    {
        return new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email =
                request.Email?.Trim() ?? EmailDetails.GenerateTempEmail(request.Username.Trim()),
            UserName = request.Username.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            PhoneNumber = request.PhoneNumber.Trim(),
            PhoneNumberConfirmed = true,
        };
    }

    public static IQueryable<UserAccountResponse> ToUsers(this IQueryable<ApplicationUser> user)
    {
        return user.Select(user => new UserAccountResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Username = user.UserName!,
            PhoneNumber = user.PhoneNumber!,
            IsActive = user.IsActive,
        });
    }
}
