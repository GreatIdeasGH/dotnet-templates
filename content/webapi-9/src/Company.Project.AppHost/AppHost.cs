using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Major Code Smell",
    "S6966:Awaitable method should be used",
    Justification = "<Pending>"
)]

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FundraiserWebAPI>("backend").WithHttpHealthCheck("/health");

// builder
//     .AddNpmApp("frontend", "../../src/app.client")
//     .WithReference(webApi)
//     .WaitFor(webApi)
//     .WithHttpEndpoint(port: 3000, targetPort: 5173, isProxied: true)
//     .WithExternalHttpEndpoints()
//     .WithHttpHealthCheck("/health")

builder.Build().Run();
