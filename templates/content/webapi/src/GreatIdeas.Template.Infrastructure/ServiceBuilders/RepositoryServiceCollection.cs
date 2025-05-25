using GreatIdeas.Template.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GreatIdeas.Template.Infrastructure.ServiceBuilders;

public static class RepositoryServiceCollection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.TryAddScoped<IUserRepository, UserRepository>();
        services.TryAddScoped<IAuditRepository, AuditRepository>();

        return services;
    }
}
