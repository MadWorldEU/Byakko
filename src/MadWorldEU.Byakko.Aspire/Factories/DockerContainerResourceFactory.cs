namespace MadWorldEU.Byakko.Factories;

internal sealed class DockerContainerResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    private const int ApiPort = 5062;
    private const int AdminPort = 5042;
    private const int PortalPort = 5100;
    
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack, IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddContainer(nameof(Api), DockerImages.ByakkoApiImage)
            .WithHttpEndpoint(targetPort: 8080, port: ApiPort)
            .BuildApi(byakkoDb, localstack, keycloak);
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddContainer(nameof(Admin), DockerImages.ByakkoAdminImage)
            .WithHttpEndpoint(targetPort: 8080, port: AdminPort)
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddContainer(nameof(Portal), DockerImages.ByakkoPortalImage)
            .WithHttpEndpoint(targetPort: 8080, port: PortalPort)
            .BuildPortal(api);
    }
}