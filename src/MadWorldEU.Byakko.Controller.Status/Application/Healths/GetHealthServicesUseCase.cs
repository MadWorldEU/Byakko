using MadWorldEU.Byakko.Domain.Healths;

namespace MadWorldEU.Byakko.Application.Healths;

internal sealed class GetHealthServicesUseCase
{
    internal IReadOnlyList<ServiceInfo> Execute()
    {
        return     [
            new("API", ServiceStatus.Healthy),
            new("Portal", ServiceStatus.Healthy),
            new("Admin", ServiceStatus.Healthy),
            new("Database", ServiceStatus.Healthy),
            new("Object Storage", ServiceStatus.Healthy),
            new("Authentication", ServiceStatus.Healthy),
        ];
    }
}