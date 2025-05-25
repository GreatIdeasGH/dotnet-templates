var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GreatIdeas_Template_WebAPI>("greatideas-template-webapi");

builder.Build().Run();
