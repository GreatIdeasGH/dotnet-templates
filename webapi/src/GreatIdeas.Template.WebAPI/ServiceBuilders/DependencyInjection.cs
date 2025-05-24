using GreatIdeas.Template.WebAPI.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

namespace GreatIdeas.Template.WebAPI.ServiceBuilders;

internal static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(
        this WebApplicationBuilder builder,
        ApplicationSettings applicationSettings
    )
    {
        // Aspire
        // builder.AddServiceDefaults(applicationSettings, ApplicationActivitySources.GetSourceNames())

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

        builder.Services.AddSwaggerOpenApiServices();

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

        // scalar
        builder.Services.AddOpenApi();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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

        // Aspire endpoints
        // app.MapDefaultEndpoints(applicationSettings)

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        // Scalar
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("GreatIdeas.Template.WebAPI")
                .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme);
        });

        app.UseCors("AllowAll");

        app.UseStatusCodePages();
        app.UseCustomExceptionHandlers();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.UseAntiforgery();

        // Map endpoints
        app.MapWebAPIEndpoints();

        // OpenAPI
        // app.UseSwaggerOpenApiServices()

        return app;
    }
}
