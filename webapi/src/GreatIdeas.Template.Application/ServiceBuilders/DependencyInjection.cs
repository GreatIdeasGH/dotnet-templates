using GreatIdeas.Template.Application.Common.Options;
using GreatIdeas.Template.Application.Features.Account.ConfirmEmail;
using GreatIdeas.Template.Application.Features.Account.ForgotPassword;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GreatIdeas.Template.Application.ServiceBuilders;

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

        // EmailSender
        builder.Services.AddSingleton<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IBlobService, BlobService>();
        builder.Services.AddScoped<ITenantService, TenantService>();

        builder.Services.TryAddScoped<SendConfirmationEmail>();
        builder.Services.TryAddScoped<SendTemporalPasswordEmail>();

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

        // return builder
        return builder;
    }
}
