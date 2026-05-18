namespace MadWorldEU.Byakko.Factories;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<TResource> BuildApi<TResource>(this IResourceBuilder<TResource> apiBuilder, IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<ILocalStackResource> localstack, IResourceBuilder<KeycloakResource> keycloak)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return apiBuilder
            .WaitFor(byakkoDb)
            .WaitFor(localstack)
            .WaitFor(keycloak)
            .WithReference(byakkoDb)
            .WithReference(localstack)
            .WithReference(keycloak)
            .WithHttpHealthCheck("/health");
    }

    internal static IResourceBuilder<TResource> BuildAdmin<TResource>(this IResourceBuilder<TResource> adminBuilder, IResourceBuilder<IResource> api)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return adminBuilder            
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }
    
    internal static IResourceBuilder<TResource> BuildPortal<TResource>(this IResourceBuilder<TResource> portalBuilder, IResourceBuilder<IResource> api)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return portalBuilder            
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }
}