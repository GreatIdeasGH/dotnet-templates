namespace GreatIdeas.Template.Application.Common;

public static class Utility
{
    public static string GetTag<T>(string functionName)
    {
        Type type = typeof(T);
        string className = type.Name;
        string tag = $"{className}.{functionName}";
        return tag;
    }

    public static string SerialNumber()
    {
        var dateTimeProvider = TimeProvider.System;
        return dateTimeProvider.GetUtcNow().ToString("yyMMddHHmmssfff");
    }

    public static string FormatPhoneNumber(this string phoneNumber, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        if (phoneNumber.AsSpan().StartsWith(countryCode))
        {
            return phoneNumber;
        }

        return $"{countryCode}{phoneNumber.AsSpan().TrimStart('0')}";
    }
}
