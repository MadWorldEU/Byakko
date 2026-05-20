namespace MadWorldEU.Byakko.Factories;

public static class DockerImages
{
    public const string LocalStackExplorer = "fgiova/localstack-explorer:1.3";
    
    public const string ByakkoApiImage = $"ghcr.io/madworldeu/byakko-api:{ByakkoTag}";
    public const string ByakkoAdminImage = $"ghcr.io/madworldeu/byakko-admin:{ByakkoTag}";
    public const string ByakkoPortalImage = $"ghcr.io/madworldeu/byakko-portal:{ByakkoTag}";
    
    private const string ByakkoTag = "v0.3.0";
}