var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter("db-username", secret: true);
var dbPassword = builder.AddParameter("db-password", secret: true);

var postgres = builder.AddPostgres("postgres", dbUsername, dbPassword)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin();

var byakkoDb = postgres.AddDatabase("byakko-db");

var minioUsername = builder.AddParameter("minio-username", secret: true);
var minioPassword = builder.AddParameter("minio-password", secret: true);

var minio = builder.AddMinioContainer("minio", minioUsername, minioPassword)
    .WithDataVolume();

var api = builder.AddProject<Api>(nameof(Api))
    .WaitFor(byakkoDb)
    .WaitFor(minio)
    .WithReference(byakkoDb)
    .WithReference(minio)
    .WithHttpHealthCheck("/health");

builder.AddProject<Admin>(nameof(Admin))
    .WaitFor(api)
    .WithHttpHealthCheck("/health.txt");

builder.AddProject<Portal>(nameof(Portal))
    .WaitFor(api)
    .WithHttpHealthCheck("/health.txt");

builder.Build().Run();