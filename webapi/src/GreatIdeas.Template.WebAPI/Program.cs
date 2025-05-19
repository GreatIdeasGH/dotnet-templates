using System.Reflection;
using GreatIdeas.Template.Application.Common.Constants;
using GreatIdeas.Template.Application.Common.Extensions;
using GreatIdeas.Template.Application.ServiceBuilders;
using GreatIdeas.Template.Infrastructure.ServiceBuilders;
using GreatIdeas.Template.WebAPI.ServiceBuilders;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting GreatIdeas.Template.WebAPI...");

var builder = WebApplication.CreateBuilder(args);

try
{
    // Setup key vault
    // builder.ConfigureKeyVault()

    // bind ApplicationSettings
    var section = builder.Configuration.GetSection(ApplicationSettings.SettingsName);
    builder
        .Services.AddOptions<ApplicationSettings>()
        .Bind(section)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    var applicationSettings = section.Get<ApplicationSettings>()!;

    // Add this after getting applicationSettings
    Log.Information(
        "Database Connection: {Connection}",
        applicationSettings.Database.PostgresConnection
    );

    // Bind EntraID
    // builder
    //     .Services.AddOptions<EntraIdOptions>()
    //     .Bind(builder.Configuration.GetSection(EntraIdOptions.SettingsName))
    //     .ValidateDataAnnotations()
    //     .ValidateOnStart()

    builder
        .AddApplicationService(applicationSettings)
        .AddInfrastructureServices(applicationSettings)
        .AddApiServices(applicationSettings);

    var app = builder.Build();

    await app.UseApiApplication(applicationSettings);
    await app.RunAsync();
}
catch (HostAbortedException ex)
{
    var message = Assembly.GetExecutingAssembly()!.GetName().Name + " processed a migration.";
    Log.Fatal(ex, message);
}
catch (Exception ex)
{
    var message = $"{ExceptionNotifications.ApplicationCrashAlert.ToString().SplitCamelCase()}";
    Log.Fatal(ex, "GreatIdeas.Template.WebAPI {Message}.", message);
    if (!builder.Environment.IsDevelopment())
    {
        // Send email
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}
