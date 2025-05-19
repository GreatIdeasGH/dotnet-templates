using GreatIdeas.Template.Application.Common.Options;
using GreatIdeas.Template.Application.Features.Account.Register;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace GreatIdeas.Template.Application.ServiceBuilders;

public static class MassTransitServiceCollection
{
    public static WebApplicationBuilder AddMassTransitServices(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        builder.Services.AddMassTransit(config =>
        {
            config.AddConsumers(typeof(AccountCreatedConsumer).Assembly);

            config.SetKebabCaseEndpointNameFormatter();

            if (applicationSettings.MassTransitSettings.UseInMemoryBus)
            {
                Log.Information("Using In-Memory Bus...");
                config.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    }
                );
            }

            if (applicationSettings.MassTransitSettings.UseAzureServiceBus)
            {
                Log.Information("Using Azure Service Bus...");
            }
        });

        return builder;
    }
}
