using Company.Project.Infrastructure.Repositories;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Company.Project.Infrastructure.ServiceBuilders;

public static class RepositoryServiceCollection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.TryAddScoped<IUserRepository, UserRepository>();
        services.TryAddScoped<IAuditRepository, AuditRepository>();
        services.TryAddScoped<IUserSessionRepository, UserSessionRepository>();

        return services;
    }
}
