namespace MadWorldEU.Byakko.Factories;

internal static class ResourceFactoryBuilder
{
    internal static IResourceFactory Create(IDistributedApplicationBuilder builder, bool useDockerFile)
    {
        if (useDockerFile)
        {
            return new DockerResourceFactory(builder);
        }
        
        return new ProjectResourceFactory(builder);
    }
}