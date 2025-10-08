using System.Threading.RateLimiting;
using Company.Project.WebAPI.Endpoints;
using Company.Project.WebAPI.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;

namespace Company.Project.WebAPI.ServiceBuilders;

internal static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        builder.Services.AddProblemDetails();

        // Timeprovider
        builder.Services.AddSingleton(TimeProvider.System);

        // Add Cors
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                config =>
                {
                    config
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("*");
                }
            );
        });

        builder.Services.AddAntiforgery();

        // Rate limiter
        builder.Services.AddRateLimiter(x =>
            x.AddFixedWindowLimiter(
                "fixed",
                options =>
                {
                    options.PermitLimit = 4;
                    options.Window = TimeSpan.FromSeconds(30);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                }
            )
        );

        // Register Microsoft OpenAPI
        builder.Services.AddOpenApi();
        builder.AddSwaggerDocs();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Register WebAPI endpoint interfaces.
        builder.Services.AddWebAPIEndpoints();

        return builder;
    }

    public static async ValueTask<WebApplication> UseApiApplication(
        this WebApplication app,
        ApplicationSettings applicationSettings
    )
    {
        // Seed database
        await SeedDatabase.MigrateDb(app, app.Environment);

        app.UseStaticFiles();

        app.UseRouting();

        // OpenAPI + Swagger/Scalar
        app.MapOpenApiWithSwagger();

        app.UseCors("AllowAll");

        app.UseStatusCodePages();
        app.UseCustomExceptionHandlers();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.UseAntiforgery();

        app.MapStaticAssets();

        // Map endpoints
        app.MapWebAPIEndpoints();

        return app;
    }
}
