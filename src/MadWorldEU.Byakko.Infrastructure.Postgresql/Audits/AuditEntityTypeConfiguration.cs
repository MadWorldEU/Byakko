using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MadWorldEU.Byakko.Audits;

internal sealed class AuditEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<Guid>(id => id.Value, id => Id.Create(id).Value);

        builder.Property(a => a.EntityType)
            .IsRequired();
        
        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasConversion<Guid>(id => id.Value, id => Id.Create(id).Value);

        builder.Property(a => a.Action)
            .IsRequired();
        
        builder.Property(a => a.IpAddress)
            .IsRequired()
            .HasConversion<string>(ip => ip.Value, ip => IpAddress.Create(ip).Value);
        
        builder.Property(a => a.OccurredBy)
            .IsRequired()
            .HasConversion<Guid>(u => u.Value, g => UserId.Create(g).Value);
        
        builder.Property(a => a.OccurredAt)
            .IsRequired();
    }
}