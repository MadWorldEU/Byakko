namespace MadWorldEU.Byakko.Factories;

internal sealed class ProjectResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack, IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddProject<Api>(nameof(Api))
            .BuildApi(byakkoDb, localstack, keycloak);
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

    public IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<IResource> api, 
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak)
    {
        return builder.AddProject<Status>(nameof(Status))
            .BuildStatus(api, byakkoDb, localstack, keycloak);
    }
}