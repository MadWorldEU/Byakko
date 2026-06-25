using MadWorldEU.Byakko.Correspondences;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers Mail infrastructure services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ICorrespondenceService"/> with an SMTP-backed <see cref="MailService"/>.</summary>
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICorrespondenceService, MailService>();
        
        return services;
    }
}