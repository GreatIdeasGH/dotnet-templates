using System.Net;

namespace Company.Project.Application.Abstractions.Services;

public interface ITenantService
{
    //IPAddress? IpAddress { get; }
    string? Name { get; }
    string? UserId { get; }
    string? Username { get; }
    string? UserAgent { get; }
    DateTimeOffset Timestamp => TimeProvider.System.GetUtcNow();
    ValueTask<IPAddress> GetIpAddress();
}
