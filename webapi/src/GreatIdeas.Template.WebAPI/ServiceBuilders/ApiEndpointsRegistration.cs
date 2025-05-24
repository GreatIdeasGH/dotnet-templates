using GreatIdeas.Template.WebAPI.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace GreatIdeas.Template.WebAPI.ServiceBuilders;

/// <summary>
/// Provides extension methods for registering and mapping API endpoints.
/// </summary>
public static class ApiEndpointsRegistration
{
    /// <summary>
    /// Maps all registered <see cref="IEndpoint"/> implementations to the specified <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to map endpoints to.</param>
    public static void MapWebAPIEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoints(app);
        }
    }

    /// <summary>
    /// Registers all non-abstract, non-interface types implementing <see cref="IEndpoint"/>
    /// in the current assembly as transient services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the endpoints to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddWebAPIEndpoints(
        this IServiceCollection services
    )
    {
        var serviceDescriptors = Assembly.GetExecutingAssembly()
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
