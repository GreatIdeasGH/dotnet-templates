using System.Reflection;

using Company.Project.Infrastructure.Data;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Company.Project.Infrastructure.ServiceBuilders;

public enum DbProviders
{
    Postgres = 1,
}

internal static class DbContextServiceCollection
{
    private static DbProviders DbProvider { get; set; } = DbProviders.Postgres;

    public static WebApplicationBuilder AddDbContextServices(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        if (DbProvider == DbProviders.Postgres)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContextFactory<ApplicationDbContext>(
                    options =>
                        options
                            .UseNpgsql(
                                applicationSettings.Database.PostgresConnection,
                                b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)
                            )
                            .EnableDetailedErrors()
                            .ConfigureWarnings(w =>
                                w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)
                            )
                            .LogTo(
                                Log.Information,
                                new[] { DbLoggerCategory.Database.Command.Name },
                                LogLevel.Information
                            ),
                    ServiceLifetime.Transient
                );
            }
            else
            {
                builder.Services.AddDbContextFactory<ApplicationDbContext>(
                    options =>
                        options
                            .UseNpgsql(
                                applicationSettings.Database.PostgresConnection,
                                b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName)
                            )
                            .LogTo(
                                Log.Error,
                                [DbLoggerCategory.Database.Command.Name],
                                LogLevel.Error
                            ),
                    ServiceLifetime.Transient
                );
            }
        }

        return builder;
    }
}
