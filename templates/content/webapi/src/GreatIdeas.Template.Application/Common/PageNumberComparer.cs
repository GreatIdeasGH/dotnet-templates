namespace GreatIdeas.Template.Application.Common;

public class PageNumberComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        // Extract the numeric part and the suffix (if any) from both strings
        var xParts = SplitPageNumber(x);
        var yParts = SplitPageNumber(y);

        // Compare the numeric parts first
        int numComparison = xParts.Number.CompareTo(yParts.Number);
        if (numComparison != 0)
            return numComparison;

        // If numeric parts are equal, compare the suffixes
        // Empty suffix comes after non-empty suffix
        if (string.IsNullOrEmpty(xParts.Suffix) && !string.IsNullOrEmpty(yParts.Suffix))
            return 1;
        if (!string.IsNullOrEmpty(xParts.Suffix) && string.IsNullOrEmpty(yParts.Suffix))
            return -1;

        // If both have suffixes or both don't have suffixes, compare lexicographically
        return string.Compare(xParts.Suffix, yParts.Suffix, StringComparison.OrdinalIgnoreCase);
    }

    private static (int Number, string Suffix) SplitPageNumber(string? page)
    {
        int i = 0;
        while (i < page?.Length && char.IsDigit(page[i]))
            i++;

        int number = int.Parse(page!.Substring(0, i));
        string suffix = i < page.Length ? page.Substring(i) : "";

        return (number, suffix);
    }
}
