using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public static class ApiEndpointsRegistration
{
    public static void UseApiEndpoints(this WebApplication app)
    {
        app.MapGroup("/")
            .WithOpenApi()
            .MapGet(
                "/",
                () =>
                    "You're running Glory Global EduTech WebAPI. Please use /docs to see the API documentation."
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
