using System.Text.Json;
using System.Text.Json.Serialization;

namespace Company.Project.Application.Services;

internal sealed class IpGeolocationService(
    HttpClient httpClient,
    ILogger<IpGeolocationService> logger
) : IIpGeolocationService
{
    private const string FreeGeoIpUrl = "http://ip-api.com/json/";

    private const string IpInfoUrl = "https://api.ip.sb/geoip";

    public async Task<IpGeolocationInfo?> GetLocationAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Try https://api.ip.sb/geoip first (free, no API key required)
            var ipInfoResult = await TryIpSbAsync(cancellationToken);
            if (ipInfoResult != null)
            {
                return ipInfoResult;
            }

            // Fallback to  ip-api.com (free, no API key required)
            var ipApiResult = await TryIpApiAsync(cancellationToken);
            if (ipApiResult != null)
            {
                return ipApiResult;
            }

            logger.LogWarning("Could not get geolocation for IP");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting geolocation for IP");
            return null;
        }
    }

    private async Task<IpGeolocationInfo?> TryIpApiAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetStringAsync(FreeGeoIpUrl, cancellationToken);
            var data = JsonSerializer.Deserialize<IpApiResponse>(response);

            if (data?.Status == "success")
            {
                return new IpGeolocationInfo
                {
                    Country = data.Country,
                    CountryCode = data.CountryCode,
                    Region = data.RegionName,
                    RegionCode = data.Region,
                    City = data.City,
                    Zip = data.Zip,
                    Latitude = data.Lat,
                    Longitude = data.Lon,
                    Timezone = data.Timezone,
                    Isp = data.Isp,
                    Organization = data.Org,
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "ip-api.com failed for IP");
        }

        return null;
    }

    private async Task<IpGeolocationInfo?> TryIpSbAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetStringAsync(IpInfoUrl, cancellationToken);

            if (response is null)
            {
                return new IpGeolocationInfo
                {
                    Country = "Local/Private",
                    City = "Local Network",
                    Region = "Private",
                };
            }

            // Deserialize the response
            var data = JsonSerializer.Deserialize<IpInfoResponse>(response);
            if (data != null && !string.IsNullOrEmpty(data.Country))
            {
                return new IpGeolocationInfo
                {
                    Country = data.Country,
                    Region = data.Region,
                    City = data.City,
                    Zip = data.ISP,
                    Latitude = data.Latitude,
                    Longitude = data.Longitude,
                    Timezone = data.Timezone,
                    Organization = data.Organization,
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "https://api.ip.sb/geoip failed for IP");
        }

        return null;
    }

    public static bool IsLocalIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return true;

        return ipAddress switch
        {
            "::1" or "127.0.0.1" or "localhost" => true,
            _ when ipAddress.StartsWith("192.168.") => true,
            _ when ipAddress.StartsWith("10.") => true,
            _ when ipAddress.StartsWith("172.") => IsPrivateClassB(ipAddress),
            _ => false,
        };
    }

    private static bool IsPrivateClassB(string ipAddress)
    {
        var parts = ipAddress.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var secondOctet))
        {
            return secondOctet is >= 16 and <= 31;
        }
        return false;
    }

    // Response models for ip-api.com
    private sealed record IpApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("regionName")]
        public string? RegionName { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("lat")]
        public decimal Lat { get; set; }

        [JsonPropertyName("lon")]
        public decimal Lon { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("isp")]
        public string? Isp { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }
    }

    // Response models for ipinfo.io
    private sealed record IpInfoResponse
    {
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("isp")]
        public string? ISP { get; set; }

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("organization")]
        public string? Organization { get; set; }
    }
}
