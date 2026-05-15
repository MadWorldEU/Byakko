namespace MadWorldEU.Byakko.Factories;

internal interface IResourceFactory
{
    internal IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio, IResourceBuilder<KeycloakResource> keycloak);
    internal IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api);
    internal IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api);
}