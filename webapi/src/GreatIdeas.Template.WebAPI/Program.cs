using GreatIdeas.Template.Application.ServiceBuilders;
using GreatIdeas.Template.Infrastructure;
using GreatIdeas.Template.Infrastructure.ServiceBuilders;
using GreatIdeas.Template.ServiceDefaults;
using GreatIdeas.Template.WebAPI.ServiceBuilders;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting GreatIdeas.Template.WebAPI...");

var builder = WebApplication.CreateBuilder(args);

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
