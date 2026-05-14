using MadWorldEU.Byakko.Factories;
using Microsoft.Extensions.Configuration;

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

var useDockerFile = builder.Configuration.GetValue<bool>("RunMode:UseDockerFile");
var resourceFactory = ResourceFactoryBuilder.Create(builder, useDockerFile);

var api = resourceFactory.CreateApiBuilder(byakkoDb, minio);

resourceFactory.CreateAdminBuilder(api);
resourceFactory.CreatePortalBuilder(api);

builder.Build().Run();
