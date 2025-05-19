using System.Net;

namespace GreatIdeas.Template.Application.Abstractions.Services;

public interface ITenantService
{
    IPAddress? IpAddress { get; }
    string? Name { get; }
    string? UserId { get; }
    string? Username { get; }
    DateTimeOffset Timestamp => TimeProvider.System.GetUtcNow();
}
