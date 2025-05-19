namespace GreatIdeas.Template.Application.Common.Constants;

public static class EmailDetails
{
    public const string FromAddress = "";
    public const string SenderName = "GGC EduTech";
    public const string BusinessNameTag = "";
    public const string TeamNameTag = "";
    public const string Domain = "ggcedutech.com";
    public const string Website = "https://ggcedutech.com";

    /// <summary>
    /// Generate a temporary email for students without email
    /// <br/>
    /// Filter tmp to display none when editing or viewing
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static string GenerateTempEmail(string username)
    {
        return $"{username}@std.{Domain}";
    }
}
