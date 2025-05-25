
namespace GreatIdeas.Template.Infrastructure.ServiceBuilders;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructureServices(
        this WebApplicationBuilder builder,
        ApplicationSettings? applicationSettings
    )
    {
        // DbContext
        builder.AddDbContextServices(applicationSettings!);

        // Register audit entries
        builder.Services.AddScoped<List<AuditEntry>>();

        // Identity
        builder.AddJwtAuthService(applicationSettings!);

        // Repositories
        builder.Services.AddRepositories();

        return builder;
    }
}
