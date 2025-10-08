using Company.Project.Application.Common.Params;

namespace Company.Project.Application.Features.Account.GetUserSessions;

public sealed record UserSessionsParameters : PagingParameters
{
    public bool? ActiveOnly { get; set; }
}

public sealed record UserSessionResponse
{
    public Guid UserSessionId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? DeviceInfo { get; init; }
    public string? Location { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? Region { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Timezone { get; init; }
    public string? Organization { get; init; }
    public DateTimeOffset LoginAt { get; init; }
    public DateTimeOffset? LogoutAt { get; init; }
    public DateTimeOffset LastActivityAt { get; init; }
    public bool IsActive { get; init; }
    public string? SessionToken { get; init; }
}

public sealed record UserSessionSummaryResponse
{
    public Guid UserSessionId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? IpAddress { get; init; }
    public DateTimeOffset LoginAt { get; init; }
    public DateTimeOffset? LogoutAt { get; init; }
    public DateTimeOffset LastActivityAt { get; init; }
    public bool IsActive { get; init; }
    public string? SessionToken { get; init; }
}
