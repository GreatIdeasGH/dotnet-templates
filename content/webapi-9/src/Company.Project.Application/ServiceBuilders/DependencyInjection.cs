using Company.Project.Application.Common.Options;
using Company.Project.Application.Features.Account.ConfirmEmail;
using Company.Project.Application.Features.Account.ForgotPassword;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Company.Project.Application.ServiceBuilders;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApplicationService(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        builder.Services.AddHttpContextAccessor();

        // FluentValidation
        builder.Services.AddValidatorsFromAssembly(typeof(IApplicationHandler).Assembly);

        builder.Services.AddScoped<IBlobService, BlobService>();
        builder.Services.AddScoped<ITenantService, TenantService>();

        // Email Services
        builder.Services.TryAddScoped<SendConfirmationEmail>();
        builder.Services.TryAddScoped<SendTemporalPasswordEmail>();

        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<ExceptionNotificationService>();

        // MassTransit
        builder.AddMassTransitServices(applicationSettings);

        // Register all Handlers
        var assembly = Assembly.GetExecutingAssembly();
        var handlerType = typeof(IApplicationHandler);
        var handlers = assembly
            .GetTypes()
            .Where(p => handlerType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();
        foreach (var handler in handlers)
        {
            var service = handler.GetInterfaces().First(i => i != handlerType);
            builder.Services.TryAddScoped(service, handler);
        }

        // IP and Session Services
        builder.Services.AddScoped<IIpGeolocationService, IpGeolocationService>();

        // HttpClient for IP Geolocation
        builder.Services.AddHttpClient<IIpGeolocationService, IpGeolocationService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "FundraiserApp/1.0");
        });

        // return builder
        return builder;
    }
}
