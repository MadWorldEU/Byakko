using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MadWorldEU.Byakko.Storages;

internal sealed class AssetEntityTypeConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<Guid>(id => id.Value, id => Id.Create(id).Value);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(Name.MaxLength)
            .HasConversion<string>(n => n.Value, s => Name.Create(s).Value);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(ContentType.MaxLength)
            .HasConversion<string>(ct => ct.Value, s => ContentType.Create(s).Value);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasConversion<Guid>(u => u.Value, g => UserId.Create(g).Value);

        builder.Property(a => a.Size)
            .HasConversion(
                s => s.Value,
                v => Size.Create(v).Value);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
    }
}