using System.Net;

namespace Company.Project.Domain.Entities;

public sealed record UserSession : EntityBase
{
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public IPAddress? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Timezone { get; set; }
    public string? Organization { get; set; }

    public DateTimeOffset LoginAt { get; set; }
    public DateTimeOffset? LogoutAt { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }

    public bool IsActive { get; set; }
    public string? SessionToken { get; set; }

    public void MarkAsLoggedOut()
    {
        LogoutAt = TimeProvider.System.GetUtcNow();
        IsActive = false;
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = TimeProvider.System.GetUtcNow();
    }

    public static UserSession CreateNew(
        string userId,
        IPAddress? ipAddress,
        string? userAgent,
        string? deviceInfo,
        string? location,
        string? country,
        string? city,
        string? region,
        decimal? latitude,
        decimal? longitude,
        string? timezone,
        string? organization
    )
    {
        return new UserSession
        {
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = deviceInfo,
            Location = location,
            Country = country,
            City = city,
            Region = region,
            Latitude = latitude,
            Longitude = longitude,
            Timezone = timezone,
            Organization = organization,
            LoginAt = TimeProvider.System.GetUtcNow(),
            LastActivityAt = TimeProvider.System.GetUtcNow(),
            IsActive = true,
        };
    }

    public static UserSession CreateNew(string userId, string userAgent, IPAddress? ipAddress)
    {
        return new UserSession
        {
            UserId = userId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            LoginAt = TimeProvider.System.GetUtcNow(),
            LastActivityAt = TimeProvider.System.GetUtcNow(),
            IsActive = true,
        };
    }
}
