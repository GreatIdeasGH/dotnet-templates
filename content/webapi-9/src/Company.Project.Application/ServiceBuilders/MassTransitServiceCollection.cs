using Company.Project.Application.Common.Options;
using Company.Project.Application.Features.Account.CreateAccount;

using Microsoft.AspNetCore.Builder;

using Serilog;

namespace Company.Project.Application.ServiceBuilders;

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

            config.DisableUsageTelemetry();

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
