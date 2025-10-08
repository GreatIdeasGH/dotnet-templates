namespace Company.Project.Application.Common.Extensions;

public static class StringExtensions
{
    public static bool StringContains(this string source, string toCheck)
    {
        return source.Contains(toCheck, StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetAbsPathFromUrl(this string url)
    {
        Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var newPath);

        var absolutePath = newPath!.AbsolutePath;
        return absolutePath.Remove(absolutePath.IndexOf('/'), 1);
    }

    public static string SplitCamelCase(this string text)
    {
        return RegexValidator.UpperCase().Replace(text, " $1").Trim();
    }

    /// <summary>
    /// Converts a string to Proper case (Title case) where the first letter of each word is capitalized
    /// and the rest are lowercase.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>A string in Proper case format</returns>
    /// <example>
    /// "hello world".ToProperCase() returns "Hello World"
    /// "JOHN DOE".ToProperCase() returns "John Doe"
    /// "mIxEd CaSe".ToProperCase() returns "Mixed Case"
    /// </example>
    public static string ToProperCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Use TextInfo.ToTitleCase which handles proper case conversion
        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(text.ToLower());
    }

    /// <summary>
    /// Converts a string to Proper case using invariant culture for consistent results
    /// regardless of the current system culture.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>A string in Proper case format using invariant culture</returns>
    /// <example>
    /// "hello world".ToProperCaseInvariant() returns "Hello World"
    /// "JOHN DOE".ToProperCaseInvariant() returns "John Doe"
    /// </example>
    public static string ToProperCaseInvariant(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Use invariant culture for consistent behavior across different systems
        var textInfo = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
        return textInfo.ToTitleCase(text.ToLower());
    }

    /// <summary>
    /// Converts a string to Proper case with custom word separators.
    /// Handles common separators like spaces, hyphens, underscores, and periods.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>A string in Proper case format with custom separator handling</returns>
    /// <example>
    /// "hello-world_test.file".ToProperCaseCustom() returns "Hello-World_Test.File"
    /// "first_name last-name".ToProperCaseCustom() returns "First_Name Last-Name"
    /// </example>
    public static string ToProperCaseCustom(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var result = new System.Text.StringBuilder(text.Length);
        bool capitalizeNext = true;

        foreach (char c in text)
        {
            if (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.')
            {
                result.Append(c);
                capitalizeNext = true;
            }
            else if (capitalizeNext)
            {
                result.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                result.Append(char.ToLower(c));
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to sentence case where the first character is capitalized
    /// and the rest are lowercase, except for known acronyms.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>A string in sentence case format</returns>
    /// <example>
    /// "HELLO WORLD".ToSentenceCase() returns "Hello world"
    /// "this is a TEST".ToSentenceCase() returns "This is a test"
    /// "API response from HTTP".ToSentenceCase() returns "API response from HTTP"
    /// </example>
    public static string ToSentenceCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Common acronyms that should remain uppercase
        var acronyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "API",
            "HTTP",
            "HTTPS",
            "URL",
            "URI",
            "JSON",
            "XML",
            "HTML",
            "CSS",
            "JS",
            "SQL",
            "DB",
            "ID",
            "UI",
            "UX",
            "AI",
            "ML",
            "IoT",
            "REST",
            "SOAP",
            "TCP",
            "UDP",
            "IP",
            "DNS",
            "SSL",
            "TLS",
            "FTP",
            "SSH",
            "VPN",
            "CPU",
            "GPU",
            "RAM",
            "SSD",
            "HDD",
            "USB",
            "WiFi",
            "GPS",
            "SMS",
            "MMS",
        };

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new System.Text.StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i].Trim();
            if (string.IsNullOrEmpty(word))
                continue;

            if (i > 0)
                result.Append(' ');

            // Check if the word is an acronym
            if (acronyms.Contains(word))
            {
                result.Append(word.ToUpper());
            }
            else if (i == 0)
            {
                // First word: capitalize first letter, lowercase the rest
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLower());
            }
            else
            {
                // Other words: all lowercase
                result.Append(word.ToLower());
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to sentence case with custom acronym list.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <param name="customAcronyms">Additional acronyms to preserve in uppercase</param>
    /// <returns>A string in sentence case format with custom acronyms preserved</returns>
    /// <example>
    /// "NASA sent API data".ToSentenceCaseWithAcronyms(new[] {"NASA"}) returns "NASA sent API data"
    /// </example>
    public static string ToSentenceCaseWithAcronyms(
        this string text,
        IEnumerable<string> customAcronyms
    )
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Default acronyms
        var acronyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "API",
            "HTTP",
            "HTTPS",
            "URL",
            "URI",
            "JSON",
            "XML",
            "HTML",
            "CSS",
            "JS",
            "SQL",
            "DB",
            "ID",
            "UI",
            "UX",
            "AI",
            "ML",
            "IoT",
            "REST",
            "SOAP",
            "TCP",
            "UDP",
            "IP",
            "DNS",
            "SSL",
            "TLS",
            "FTP",
            "SSH",
            "VPN",
            "CPU",
            "GPU",
            "RAM",
            "SSD",
            "HDD",
            "USB",
            "WiFi",
            "GPS",
            "SMS",
            "MMS",
        };

        // Add custom acronyms
        if (customAcronyms != null)
        {
            foreach (
                var acronym in from acronym in customAcronyms
                where !string.IsNullOrWhiteSpace(acronym)
                select acronym
            )
            {
                acronyms.Add(acronym);
            }
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new System.Text.StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i].Trim();
            if (string.IsNullOrEmpty(word))
                continue;

            if (i > 0)
                result.Append(' ');

            // Check if the word is an acronym
            if (acronyms.Contains(word))
            {
                result.Append(word.ToUpper());
            }
            else if (i == 0)
            {
                // First word: capitalize first letter, lowercase the rest
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLower());
            }
            else
            {
                // Other words: all lowercase
                result.Append(word.ToLower());
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to sentence case without preserving any acronyms.
    /// Only the first character is capitalized, everything else is lowercase.
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>A string in simple sentence case format</returns>
    /// <example>
    /// "HELLO WORLD API".ToSimpleSentenceCase() returns "Hello world api"
    /// "This Is A Test".ToSimpleSentenceCase() returns "This is a test"
    /// </example>
    public static string ToSimpleSentenceCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var trimmed = text.Trim();
        if (trimmed.Length == 0)
            return trimmed;

        return char.ToUpper(trimmed[0])
            + (trimmed.Length > 1 ? trimmed.Substring(1).ToLower() : "");
    }
}
