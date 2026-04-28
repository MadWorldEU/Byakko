using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Admin>(nameof(Admin));
builder.AddProject<Api>(nameof(Api));
builder.AddProject<Portal>(nameof(Portal));

builder.Build().Run();