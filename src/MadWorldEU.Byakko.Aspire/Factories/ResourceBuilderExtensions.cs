namespace MadWorldEU.Byakko.Factories;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<TResource> BuildApi<TResource>(this IResourceBuilder<TResource> apiBuilder, IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio) 
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return apiBuilder            
            .WaitFor(byakkoDb)
            .WaitFor(minio)
            .WithReference(byakkoDb)
            .WithReference(minio)
            .WithHttpHealthCheck("/health");
    }

    public static IResourceBuilder<TResource> BuildAdmin<TResource>(this IResourceBuilder<TResource> adminBuilder, IResourceBuilder<IResource> api)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return adminBuilder            
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }
    
    public static IResourceBuilder<TResource> BuildPortal<TResource>(this IResourceBuilder<TResource> portalBuilder, IResourceBuilder<IResource> api)
        where TResource : IResource, IResourceWithWaitSupport, IResourceWithEnvironment, IResourceWithEndpoints
    {
        return portalBuilder            
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }
}