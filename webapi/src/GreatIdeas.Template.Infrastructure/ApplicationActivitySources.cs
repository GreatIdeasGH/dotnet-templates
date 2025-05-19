using System.Reflection;

namespace GreatIdeas.Template.Infrastructure;

public static class ApplicationActivitySources
{
    public static string[] GetSourceNames()
    {
        // Get all names for implementations using IRequestCommandHandler
        var requestHandlers = typeof(ITenantService)
            .Assembly.GetTypes()
            .Where(t =>
                t.GetInterfaces()
                    .Where(i => i.Name.Contains("Handler") || i.Name.Contains("Consumer"))
                    .ToList()
                    .Count > 0
            )
            .Select(n => n.Name)
            .ToList();

        // Get all names for implementations using IRepository
        var repositories = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.GetInterfaces().Where(i => i.Name.Contains("Repository")).ToList().Count > 0
            )
            .Select(n => n.Name)
            .ToList();

        // Merge the lists
        var activitySourceNames = requestHandlers
            .Concat(repositories)
            .Concat(
                new List<string>
                {
                    "ExportFileHelper",
                    "ErrorHandlerEndpoint",
                    "SendConfirmationEmail",
                    "SeedDatabase",
                    "SeedSamples",
                    "CognitiveEndpoints",
                    "TranslatorServices"
                }
            )
            .ToList();

        return activitySourceNames.ToArray();
    }
}
