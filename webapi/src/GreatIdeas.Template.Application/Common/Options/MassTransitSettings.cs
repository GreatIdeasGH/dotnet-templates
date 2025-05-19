namespace GreatIdeas.Template.Application.Common.Options;

public struct MassTransitSettings
{
    public bool UseAzureServiceBus { get; set; }
    public bool UseInMemoryBus { get; set; }
    public string? RabbitMqHost { get; set; }
    public ushort RabbitMqPort { get; set; }
    public string? RabbitMqUsername { get; set; }
    public string? RabbitMqPassword { get; set; }
}
