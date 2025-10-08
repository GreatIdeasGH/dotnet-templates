using System.Reflection;

namespace Company.Project.Infrastructure;

public static class ApplicationActivitySources
{
    public static string[] GetSourceNames()
    {
        // Get all names for implementations using IApplicationHandler
        var requestHandlers = typeof(IApplicationHandler)
            .Assembly.GetTypes()
            .Where(t =>
                t.GetInterfaces()
                    .Where(i =>
                        i.Name.Contains("Handler")
                        || i.Name.Contains("Consumer")
                        || i.Name.Contains("Service")
                    )
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
            .Concat(["ExportFileHelper", "ErrorHandlerEndpoint", "SendConfirmationEmail"])
            .ToList();

        return activitySourceNames.ToArray();
    }
}
