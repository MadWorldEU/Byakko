namespace MadWorldEU.Byakko.Factories;

public class DockerResourceFactory(IDistributedApplicationBuilder builder) : IResourceFactory
{
    public IResourceBuilder<IResource> CreateApiBuilder(IResourceBuilder<PostgresDatabaseResource> byakkoDb, IResourceBuilder<MinioContainerResource> minio)
    {
        return builder.AddDockerfile(nameof(Api), "../../", "src/MadWorldEU.Byakko.Controller.Api/Dockerfile")
            .WithHttpEndpoint(targetPort: 8080)
            .BuildApi(byakkoDb, minio);       
    }

    public IResourceBuilder<IResource> CreateAdminBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Admin), "../../", "src/MadWorldEU.Byakko.Controller.Admin/Dockerfile")
            .WithHttpEndpoint(targetPort: 80)
            .BuildAdmin(api);
    }

    public IResourceBuilder<IResource> CreatePortalBuilder(IResourceBuilder<IResource> api)
    {
        return builder.AddDockerfile(nameof(Portal), "../../", "src/MadWorldEU.Byakko.Controller.Portal/Dockerfile")
            .WithHttpEndpoint(targetPort: 80)
            .BuildPortal(api);
    }
}