namespace MadWorldEU.Byakko.Factories;

public interface IResourceFactory
{
    IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio, IResourceBuilder<KeycloakResource> keycloak);
    IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api);
    IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api);
}