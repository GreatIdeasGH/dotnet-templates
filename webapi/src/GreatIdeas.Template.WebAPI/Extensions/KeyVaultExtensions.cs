using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace GreatIdeas.Template.WebAPI.Extensions;

public static class KeyVaultExtensions
{
    public static void ConfigureKeyVault(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var keyVaultName = configuration["ApplicationSettings:AzureSettings:KeyVault"];
        var clientId = configuration["AzureAd:ClientId"];
        var tenantId = configuration["AzureAd:TenantId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];

        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new ClientSecretCredential(
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecret
            ),
            new AzureKeyVaultConfigurationOptions { ReloadInterval = TimeSpan.FromDays(1) }
        );
    }
}

class SampleKeyVaultSecretManager : KeyVaultSecretManager
{
    public override bool Load(SecretProperties secret) =>
        secret.ExpiresOn.HasValue && secret.ExpiresOn.Value > DateTimeOffset.Now;
}
