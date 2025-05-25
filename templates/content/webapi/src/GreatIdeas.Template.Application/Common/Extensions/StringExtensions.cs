namespace GreatIdeas.Template.Application.Common.Extensions;

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
}
