using NodaTime;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers building block services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
        
        return services;
    }
}