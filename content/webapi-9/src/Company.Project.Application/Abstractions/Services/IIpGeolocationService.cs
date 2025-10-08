namespace Company.Project.Application.Abstractions.Services;

public interface IIpGeolocationService
{
    Task<IpGeolocationInfo?> GetLocationAsync(CancellationToken cancellationToken = default);
}

public sealed record IpGeolocationInfo
{
    public string? Country { get; init; }
    public string? CountryCode { get; init; }
    public string? Region { get; init; }
    public string? RegionCode { get; init; }
    public string? City { get; init; }
    public string? Zip { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Timezone { get; init; }
    public string? Isp { get; init; }
    public string? Organization { get; init; }
    public string? FullLocation => GetFullLocation();

    private string GetFullLocation()
    {
        if (
            !string.IsNullOrEmpty(City)
            && !string.IsNullOrEmpty(Region)
            && !string.IsNullOrEmpty(Country)
        )
            return $"{City}, {Region}, {Country}";

        if (!string.IsNullOrEmpty(Region) && !string.IsNullOrEmpty(Country))
            return $"{Region}, {Country}";

        return Country ?? "Unknown";
    }
}
