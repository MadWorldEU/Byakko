namespace MadWorldEU.Byakko.Factories;

internal sealed class DockerContainerResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    private const int ApiPort = 5062;
    private const int AdminPort = 5042;
    private const int PortalPort = 5100;
    private const int StatusPort = 5063;
    
    public IResourceBuilder<IResource> CreateApiBuilder(
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
    {
        return builder.AddContainer(nameof(Api), DockerImages.ByakkoApiImage)
            .WithHttpEndpoint(targetPort: 8080, port: ApiPort)
            .BuildApi(byakkoDb, localstack, keycloak, mailPit);
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
            .BuildPortal(api)
            .BuildDockerPortal(PortalPort);       
    }

    public IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack,
        IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddContainer(nameof(Status), DockerImages.ByakkoStatusImage)
            .WithHttpEndpoint(targetPort: 8080, port: StatusPort)
            .BuildStatus(byakkoDb, localstack, keycloak);
    }
}