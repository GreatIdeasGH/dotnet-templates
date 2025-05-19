namespace GreatIdeas.Template.Application.Common.Options;

public struct AzureSettings
{
    public AzureKeyVaultSettings KeyVault { get; set; }
    public AzureBlobStorageSettings BlobStorage { get; set; }
    public AzureKeyRegionSettings Speech { get; set; }
    public AzureKeyRegionSettings Translator { get; set; }
    public string? ApplicationInsightsUri { get; set; }
    public string? ServiceBusUri { get; set; }
}

public struct AzureBlobStorageSettings
{
    public string AccountKey { get; set; }
    public string ContainerName { get; set; }
    public string AccountName { get; set; }
    public string Url { get; set; }
}

public struct AzureKeyVaultSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string VaultUri { get; set; }
}

public struct AzureKeyRegionSettings
{
    public string Key { get; set; }
    public string Region { get; set; }
}
