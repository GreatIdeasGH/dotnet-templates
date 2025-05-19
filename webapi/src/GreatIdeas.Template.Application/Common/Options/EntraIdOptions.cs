namespace GreatIdeas.Template.Application.Common.Options;

public sealed record EntraIdOptions
{
    public const string SettingsName = "AzureAd";
    public string TenantId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string UserObjectId { get; set; } = default!;
    public string[] Scopes { get; set; } = [];
}