using MadWorldEU.Byakko.Factories;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var byakkoDb = builder.BuildDatabase();
var localStack = builder.BuildLocalStack();
var keycloak = builder.BuildKeyCloak();

var runMode = builder.Configuration.GetValue<RunMode>("RunMode");
var resourceFactory = ResourceFactoryBuilder.Create(builder, runMode);

var api = resourceFactory.CreateApiBuilder(byakkoDb, localStack, keycloak);
resourceFactory.CreateAdminBuilder(api);
resourceFactory.CreatePortalBuilder(api);

await builder.Build().RunAsync();
