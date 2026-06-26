namespace MadWorldEU.Byakko.Factories;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<TResource> BuildApi<TResource>(
        this IResourceBuilder<TResource> apiBuilder, 
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<MailPitContainerResource> mailPit)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return apiBuilder
            .WaitFor(byakkoDb)
            .WaitFor(localstack)
            .WaitFor(keycloak)
            .WaitFor(mailPit)
            .WithReference(byakkoDb)
            .WithReference(localstack)
            .WithReference(keycloak)
            .WithReference(mailPit)
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
    
    internal static IResourceBuilder<TResource> BuildDockerPortal<TResource>(this IResourceBuilder<TResource> portalBuilder, int portalPort)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return portalBuilder            
            .WithEnvironment("OG_IMAGE_URL", "http://localhost:"  + portalPort + "/images/byakko-header.png");
    }
    
    internal static IResourceBuilder<TResource> BuildStatus<TResource>(this IResourceBuilder<TResource> statusBuilder, 
        IResourceBuilder<PostgresDatabaseResource> byakkoDb, 
        IResourceBuilder<ILocalStackResource> localstack, 
        IResourceBuilder<KeycloakResource> keycloak)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return statusBuilder
            .WithReference(byakkoDb)
            .WithReference(localstack)
            .WithReference(keycloak)
            .WithHttpHealthCheck("/health");
    }
}