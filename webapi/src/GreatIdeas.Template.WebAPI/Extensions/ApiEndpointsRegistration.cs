using System.Reflection;
using GreatIdeas.Template.WebAPI.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GreatIdeas.Template.WebAPI.Extensions;

public static class ApiEndpointsRegistration
{
    public static void UseApiEndpoints(this WebApplication app)
    {
        app.MapGroup("/")
            .WithOpenApi()
            .MapGet(
                "/",
                () =>
                    "You're running GreatIdeas.Template.WebAPI. Please use /docs to see Swagger API documentation."
            )
            .ExcludeFromDescription();

        app.MapGet("/error", ErrorHandlerEndpoint.MapErrorHandler).ExcludeFromDescription();

        // Register endpoints with IEndpoint
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoints(app);
        }
    }

    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var serviceDescriptors = assembly
            .DefinedTypes.Where(type =>
                type is { IsAbstract: false, IsInterface: false }
                && type.IsAssignableTo(typeof(IEndpoint))
            )
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }
}
