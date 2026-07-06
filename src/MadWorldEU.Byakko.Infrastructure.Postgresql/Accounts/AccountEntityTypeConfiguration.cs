using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MadWorldEU.Byakko.Accounts;

/// <summary>EF Core entity type configuration for <see cref="Account"/>.</summary>
public sealed class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    /// <summary>Configures the column mappings and value converters for <see cref="Account"/>.</summary>
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<Guid>(id => id.Value, id => Id.Create(id).Value);

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasConversion<Guid>(id => id.Value, id => UserId.Create(id).Value);
        
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();
        
        builder.HasIndex(a => a.UserId).IsUnique();
    }
}