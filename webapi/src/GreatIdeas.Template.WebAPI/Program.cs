using GreatIdeas.Template.Application.ServiceBuilders;
using GreatIdeas.Template.Infrastructure.ServiceBuilders;
using GreatIdeas.Template.WebAPI.ServiceBuilders;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting GreatIdeas.Template.WebAPI...");

var builder = WebApplication.CreateBuilder(args);

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
