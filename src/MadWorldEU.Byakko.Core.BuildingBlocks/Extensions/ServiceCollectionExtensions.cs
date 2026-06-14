using MadWorldEU.Byakko.Systems;
using NodaTime;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers building block services with the dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocks(this IServiceCollection services)
    {
        services.AddEventDrivenDevelopment();
        services.AddSystems();
        
        return services;
    }

    private static void AddEventDrivenDevelopment(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();
    }
    
    private static void AddSystems(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IGuidGenerator, GuidGenerator>();
    }
}