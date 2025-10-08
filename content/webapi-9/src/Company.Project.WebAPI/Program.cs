using Company.Project.WebAPI.ServiceBuilders;

using Company.Project.Application.ServiceBuilders;
using Company.Project.Infrastructure;
using Company.Project.Infrastructure.ServiceBuilders;
using Company.Project.ServiceDefaults;

using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting Fundraiser WebAPI...");

var builder = WebApplication.CreateBuilder(args);

try
{
    // Aspire service defaults with ActivitySources
    builder.AddServiceDefaults(ApplicationActivitySources.GetSourceNames());

    // bind ApplicationSettings
    var section = builder.Configuration.GetSection(ApplicationSettings.SettingsName);
    builder
        .Services.AddOptions<ApplicationSettings>()
        .Bind(section)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    var applicationSettings = section.Get<ApplicationSettings>()!;

    builder
        .AddApplicationService(applicationSettings)
        .AddInfrastructureServices(applicationSettings)
        .AddApiServices(applicationSettings);

    var app = builder.Build();

    // Aspire endpoints
    app.MapDefaultEndpoints();

    await app.UseApiApplication(applicationSettings);
    await app.RunAsync();
}
catch (HostAbortedException)
{
    Log.Warning("Fundraiser WebAPI host aborted!");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fundraiser WebAPI terminated unexpectedly!");
}
finally
{
    await Log.CloseAndFlushAsync();
}
