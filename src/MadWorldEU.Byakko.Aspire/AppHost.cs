using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin();

var byakkoDb = postgres.AddDatabase("byakko-db");

var api = builder.AddProject<Api>(nameof(Api))
    .WaitFor(byakkoDb)
    .WithReference(byakkoDb);

builder.AddProject<Admin>(nameof(Admin))
    .WaitFor(api);

builder.AddProject<Portal>(nameof(Portal))
    .WaitFor(api);

builder.Build().Run();