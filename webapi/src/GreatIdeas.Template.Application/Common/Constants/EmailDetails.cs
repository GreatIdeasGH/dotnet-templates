namespace GreatIdeas.Template.Application.Common.Constants;

public static class EmailDetails
{
    public const string FromAddress = "";
    public const string SenderName = "GreatIdeas.Templates.WebAPI";
    public const string BusinessNameTag = "";
    public const string TeamNameTag = "";
    public const string Domain = "domain.com";
    public const string Website = "https://domain.com";

    public static string GenerateTempEmail(string username)
    {
        return $"{username}@{Domain}";
    }
}
