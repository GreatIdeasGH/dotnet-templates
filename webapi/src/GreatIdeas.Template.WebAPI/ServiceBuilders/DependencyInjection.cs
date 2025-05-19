using GreatIdeas.Template.WebAPI.Endpoints;
using GreatIdeas.Template.WebAPI.OpenApi;

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
        builder.Services.AddEndpoints(typeof(IEndpoint).Assembly);
        builder.Services.AddControllers();
        builder.Services.AddAntiforgery();

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

        app.UseCors("AllowAll");

        app.UseStatusCodePages();
        app.UseCustomExceptionHandlers();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.UseApiEndpoints();
        app.MapControllers();

        // OpenAPI
        app.UseSwaggerOpenApiServices();

        return app;
    }
}
