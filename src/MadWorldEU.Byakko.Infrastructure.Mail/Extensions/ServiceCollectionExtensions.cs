using MadWorldEU.Byakko.Connections;
using MadWorldEU.Byakko.Correspondences;
using Microsoft.Extensions.Options;

namespace MadWorldEU.Byakko.Extensions;

/// <summary>Registers Mail infrastructure services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ICorrespondenceService"/> with an SMTP-backed <see cref="MailService"/>.</summary>
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOptions<MailOptions>>(sp =>
            Options.Create(MailFactory.Create(sp.GetRequiredService<IConfiguration>())));
        
        services.AddScoped<MailContext>();
        services.AddScoped<ICorrespondenceService, MailService>();
        
        return services;
    }
}