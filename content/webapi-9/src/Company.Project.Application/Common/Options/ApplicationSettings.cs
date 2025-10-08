using Microsoft.Extensions.Options;

namespace Company.Project.Application.Common.Options;

public sealed record ApplicationSettings
{
    public const string SettingsName = "ApplicationSettings";

    public string? WebUrl { get; set; }
    public bool UseOtlp { get; set; }
    public bool UseRedis { get; set; }
    public bool EnableMetrics { get; set; }
    public bool UseAzureMonitor { get; set; }
    public Guid ApiKey { get; set; }
    public OtlpOptions Otlp { get; set; }
    public DatabaseOptions Database { get; set; }

    [ValidateObjectMembers]
    public JwtSettings JwtSettings { get; set; }
    public MassTransitSettings MassTransitSettings { get; set; }
    public AzureSettings AzureSettings { get; set; }
    public PaystackSettings PaystackSettings { get; set; }
    public EmailSettings EmailSettings { get; set; }
}

public struct DatabaseOptions
{
    public string PostgresConnection { get; set; }
    public string RedisConnection { get; set; }
}

[OptionsValidator]
public partial class ApplicationSettingsValidator : IValidateOptions<ApplicationSettings> { }
