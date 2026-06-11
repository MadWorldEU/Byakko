namespace MadWorldEU.Byakko.Factories;

internal interface IResourceFactory
{
    internal IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak);
    
    internal IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api);
    internal IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api);

    internal IResourceBuilder<IResource> CreateStatusBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb,
        IResourceBuilder<ILocalStackResource> localstack,
        IResourceBuilder<KeycloakResource> keycloak);
}