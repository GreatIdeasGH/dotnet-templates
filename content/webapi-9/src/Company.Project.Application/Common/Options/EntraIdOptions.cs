namespace Company.Project.Application.Common.Options;

public sealed record EntraIdOptions
{
    public const string SettingsName = "AzureAd";
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string UserObjectId { get; set; } = null!;
    public string[] Scopes { get; set; } = [];
}
