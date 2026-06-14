using MadWorldEU.Byakko.DomainDrivenDevelopment;

namespace MadWorldEU.Byakko.Audits;

public sealed class AuditAssetsHandler 
    : IDomainEventHandler<AssetMetaDataCreatedEvent>,
        IDomainEventHandler<AssetContentUploadedEvent>
{
    public Task Handle(AssetMetaDataCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Handle(AssetContentUploadedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}