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
    }
}