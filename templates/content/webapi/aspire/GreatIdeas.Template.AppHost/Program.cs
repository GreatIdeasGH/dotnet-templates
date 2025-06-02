using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Major Code Smell",
    "S6966:Awaitable method should be used",
    Justification = "<Pending>"
)]

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GreatIdeas_Template_WebAPI>("greatideas-template-webapi");

builder.Build().Run();
