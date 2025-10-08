using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Http;

namespace Company.Project.Application.Services;

public sealed class TenantService : ITenantService
{
    //public IPAddress? IpAddress => GetIpAddress();
    public string? Name { get; }
    public string? UserId { get; }
    public string? Username { get; }
    public string? UserAgent { get; }

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        var fullName = GetClaimValue(httpContextAccessor, "name");
        var username = GetClaimValue(httpContextAccessor, "username");
        var userId = GetClaimValue(httpContextAccessor, "id");
        var userAgent = httpContextAccessor.HttpContext?.Request?.Headers?.UserAgent.ToString();

        Name = fullName ?? "System";
        UserId = userId;
        Username = username;
        UserAgent = userAgent;
    }

    private static string? GetClaimValue(IHttpContextAccessor httpContextAccessor, string claimType)
    {
        return httpContextAccessor
            .HttpContext?.User?.Claims!.FirstOrDefault(c => c.Type == claimType)
            ?.Value;
    }

    public async ValueTask<IPAddress> GetIpAddress()
    {
        try
        {
            const string url = "https://api.ipify.org/";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            //Get IP address from response
            response ??= "127.0.0.1";
            return IPAddress.Parse(response);
        }
        catch (SocketException ex)
        {
            // Log exception or handle it as needed
            Console.WriteLine($"SocketException: {ex.Message}");
            return IPAddress.Parse("127.0.0.1");
        }
        catch (Exception)
        {
            return IPAddress.Parse("127.0.0.1");
        }
    }
}
