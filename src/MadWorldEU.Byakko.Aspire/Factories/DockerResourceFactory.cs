namespace MadWorldEU.Byakko.Factories;

internal sealed class DockerResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack, IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddDockerfile(nameof(Api), "../../", "src/MadWorldEU.Byakko.Controller.Api/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080)
            .BuildApi(byakkoDb, localstack, keycloak);
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Admin), "../../", "src/MadWorldEU.Byakko.Controller.Admin/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080)
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Portal), "../../", "src/MadWorldEU.Byakko.Controller.Portal/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080)
            .BuildPortal(api);
    }
}