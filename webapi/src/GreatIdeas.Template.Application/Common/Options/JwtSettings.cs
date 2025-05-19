using System.ComponentModel.DataAnnotations;

namespace GreatIdeas.Template.Application.Common.Options;

public struct JwtSettings
{
    public string Secret { get; set; }
    public string ValidAudience { get; set; }
    public string ValidIssuer { get; set; }
    public int LogoutTimeInMinutes { get; set; }

    [Range(1, 24 * 7)]
    public int ExpiryTimeInHours { get; set; }

    [Range(1, 7)]
    public int RefreshTokenExpiryInDays { get; set; }
}
