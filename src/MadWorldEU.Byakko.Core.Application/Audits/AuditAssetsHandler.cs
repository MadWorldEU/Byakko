using MadWorldEU.Byakko.DomainDrivenDevelopment;
using MadWorldEU.Byakko.Systems;

namespace MadWorldEU.Byakko.Audits;

/// <summary>Handles asset domain events by writing an audit log entry for each action.</summary>
public sealed class AuditAssetsHandler(
    IClock clock,
    IGuidGenerator guidGenerator,
    IAuditRepository auditRepository,
    ILogger<AuditAssetsHandler> logger)
    : IDomainEventHandler<AssetMetaDataCreatedEvent>,
        IDomainEventHandler<AssetContentUploadedEvent>
{
    public async Task Handle(AssetMetaDataCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditResult = AuditLog.Create(clock, guidGenerator, domainEvent.AssetId, AuditAction.Created, domainEvent.IpAddress, domainEvent.CreatedBy);
        if (auditResult.IsFailure)
        {
            logger.LogWarning("Failed to create audit log for asset '{AssetId}': {Error}", domainEvent.AssetId, auditResult.Error.Description);
            return;
        }

        await auditRepository.AddAsync(auditResult.Value);
    }

    public async Task Handle(AssetContentUploadedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditResult = AuditLog.Create(clock, guidGenerator, domainEvent.AssetId, AuditAction.Uploaded, domainEvent.IpAddress, domainEvent.CreatedBy);
        if (auditResult.IsFailure)
        {
            logger.LogWarning("Failed to create audit log for asset '{AssetId}': {Error}", domainEvent.AssetId, auditResult.Error.Description);
            return;
        }

        await auditRepository.AddAsync(auditResult.Value);
    }
}