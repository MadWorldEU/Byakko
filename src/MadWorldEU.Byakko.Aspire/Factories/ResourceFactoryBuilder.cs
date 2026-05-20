namespace MadWorldEU.Byakko.Factories;

internal static class ResourceFactoryBuilder
{
    internal static IResourceFactory Create(IDistributedApplicationBuilder builder, RunMode mode)
    {
        return mode switch
        {
            RunMode.DockerFile => new DockerFileResourceFactory(builder),
            RunMode.ContainerImage => new DockerContainerResourceFactory(builder),
            RunMode.Project => new ProjectResourceFactory(builder),
            _ => throw new InvalidOperationException($"Unknown RunMode: {mode}")
        };
    }
}