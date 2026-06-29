namespace MadWorldEU.Byakko.Factories;

internal sealed class ProjectResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
    {
        return builder.AddProject<Api>(nameof(Api))
            .BuildApi(byakkoDb, localstack, keycloak, mailPit);
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddProject<Admin>(nameof(Admin))
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddProject<Portal>(nameof(Portal))
            .BuildPortal(api);
    }

    public IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
    {
        return builder.AddProject<Status>(nameof(Status))
            .BuildStatus(byakkoDb, localstack, keycloak, mailPit);
    }
}