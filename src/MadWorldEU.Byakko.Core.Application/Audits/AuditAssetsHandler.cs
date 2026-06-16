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
        IDomainEventHandler<AssetContentDeletedEvent>,
        IDomainEventHandler<AssetContentUploadedEvent>
{
    public async Task Handle(AssetMetaDataCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await CreateLog(domainEvent.AssetId, AuditAction.Created, domainEvent.IpAddress, domainEvent.CreatedBy);
    }

    public async Task Handle(AssetContentUploadedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await CreateLog(domainEvent.AssetId, AuditAction.Uploaded, domainEvent.IpAddress, domainEvent.CreatedBy);
    }

    public async Task Handle(AssetContentDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await CreateLog(domainEvent.AssetId, AuditAction.SoftDeleted, domainEvent.IpAddress, domainEvent.CreatedBy);
    }

    private async Task CreateLog(Id assetId, AuditAction action, IpAddress ipAddress, UserId createdBy)
    {
        var auditResult = AuditLog.Create(clock, guidGenerator, assetId, action, ipAddress, createdBy);
        if (auditResult.IsFailure)
        {
            logger.LogWarning("Failed to create audit log for asset '{AssetId}': {Error}", assetId, auditResult.Error.Description);
            return;
        }

        await auditRepository.AddAsync(auditResult.Value);
    }
}