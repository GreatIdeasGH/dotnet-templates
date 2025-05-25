using Microsoft.AspNetCore.Http;
using System.Net;

namespace GreatIdeas.Template.Application.Services;

internal sealed class TenantService : ITenantService
{
    public IPAddress? IpAddress { get;  }
    public string? Name { get;  } 
    public string? UserId { get;  }
    public string? Username { get;  }

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress!;
        var fullName = GetClaimValue(httpContextAccessor, "name");
        var username = GetClaimValue(httpContextAccessor, "username");
        var userId = GetClaimValue(httpContextAccessor, "id");


        IpAddress = ipAddress;
        Name = fullName ?? "System";
        UserId = userId;
        Username = username;
    }

    private static string? GetClaimValue(IHttpContextAccessor httpContextAccessor, string claimType)
    {
        return httpContextAccessor.HttpContext?.User?.Claims!.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
