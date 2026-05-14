namespace MadWorldEU.Byakko.Factories;

public static class ResourceFactoryBuilder
{
    public static IResourceFactory Create(IDistributedApplicationBuilder builder, bool useDockerFile)
    {
        if (useDockerFile)
        {
            return new DockerResourceFactory(builder);
        }
        
        return new ProjectResourceFactory(builder);
    }
}