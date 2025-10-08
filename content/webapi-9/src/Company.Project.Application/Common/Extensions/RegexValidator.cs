using System.Text.RegularExpressions;

namespace Company.Project.Application.Common.Extensions;

public static partial class RegexValidator
{
    [GeneratedRegex(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$")]
    public static partial Regex TimeRegex();

    // Spaces not allowed
    [GeneratedRegex(@"^\S*$")]
    public static partial Regex NoSpacesRegex();

    // Phone number regex validator for 10 digits
    [GeneratedRegex(@"^\d{10}$")]
    public static partial Regex PhoneNumberRegex();

    // Username regex validator for no spaces, no special characters
    [GeneratedRegex(@"^[a-zA-Z0-9]*$")]
    public static partial Regex UsernameRegex();

    // String must be uppercase
    [GeneratedRegex("([A-Z])", RegexOptions.Compiled)]
    public static partial Regex UpperCase();

    // Enter a valid image URL
    [GeneratedRegex(@".*\.(?:png|jpg|jpeg|webp|avif)$", RegexOptions.IgnoreCase)]
    public static partial Regex ImageUrlRegex();
}
