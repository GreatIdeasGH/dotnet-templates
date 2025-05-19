using System.Text.RegularExpressions;

namespace GreatIdeas.Template.Application.Common.Extensions;

public static partial class RegexValidator
{
    [GeneratedRegex(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$")]
    public static partial Regex TimeRegex();

    // Spaces not allowed
    [GeneratedRegex(@"^\S*$")]
    public static partial Regex NoSpacesRegex();

    // Phone number regex validator for 10 digits or max 15 digits
    [GeneratedRegex(@"^\d{10,15}$")]
    public static partial Regex PhoneNumberRegex();

    // Username regex validator for no spaces, no special characters
    [GeneratedRegex(@"^[a-zA-Z0-9]*$")]
    public static partial Regex UsernameRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9\-_]+$")]
    public static partial Regex NumberFormatRegex();

    [GeneratedRegex(@"^[0-9][0-9a-z]{0,2}[A-Z]?$")]
    public static partial Regex PageNumberRegex();

    [GeneratedRegex("([A-Z])", RegexOptions.Compiled)]
    public static partial Regex UpperCase();
}
