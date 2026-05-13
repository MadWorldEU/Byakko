using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MadWorldEU.Byakko.Storages;

public class AssetEntityTypeConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(Name.MaxLength)
            .HasConversion<string>(n => n.Value, s => Name.Create(s).Value);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(ContentType.MaxLength)
            .HasConversion<string>(ct => ct.Value, s => ContentType.Create(s).Value);
    }
}