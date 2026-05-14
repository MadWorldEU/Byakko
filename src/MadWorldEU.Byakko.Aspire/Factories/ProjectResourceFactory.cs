namespace MadWorldEU.Byakko.Factories;

public class ProjectResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio)
    {
        return builder.AddProject<Api>(nameof(Api))
            .WaitFor(byakkoDb)
            .WaitFor(minio)
            .WithReference(byakkoDb)
            .WithReference(minio)
            .WithHttpHealthCheck("/health");
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddProject<Admin>(nameof(Admin))
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddProject<Portal>(nameof(Portal))
            .WaitFor(api)
            .WithHttpHealthCheck("/health.txt");
    }
}