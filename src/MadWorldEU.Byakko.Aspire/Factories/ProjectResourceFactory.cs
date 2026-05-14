namespace MadWorldEU.Byakko.Factories;

public sealed class ProjectResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio)
    {
        return builder.AddProject<Api>(nameof(Api))
            .BuildApi(byakkoDb, minio);
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
}