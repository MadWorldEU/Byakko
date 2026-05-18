using MadWorldEU.Byakko.Factories;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var byakkoDb = builder.BuildDatabase();
var localStack = builder.BuildLocalStack();
var keycloak = builder.BuildKeyCloak();

var useDockerFile = builder.Configuration.GetValue<bool>("RunMode:UseDockerFile");
var resourceFactory = ResourceFactoryBuilder.Create(builder, useDockerFile);

var api = resourceFactory.CreateApiBuilder(byakkoDb, localStack, keycloak);
resourceFactory.CreateAdminBuilder(api);
resourceFactory.CreatePortalBuilder(api);

await builder.Build().RunAsync();
