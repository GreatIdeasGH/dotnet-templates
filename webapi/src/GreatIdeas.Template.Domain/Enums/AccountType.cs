using Ardalis.SmartEnum;

namespace GreatIdeas.Template.Domain.Enums;

public sealed class AccountType : SmartEnum<AccountType>
{
    public static readonly AccountType User = new(nameof(User), 1);
    public static readonly AccountType Admin = new(nameof(Admin), 2);

    private AccountType(string name, int value)
        : base(name, value) { }
}
