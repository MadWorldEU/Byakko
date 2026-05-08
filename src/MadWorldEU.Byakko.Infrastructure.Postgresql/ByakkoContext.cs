using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko;

public sealed class ByakkoContext(DbContextOptions<ByakkoContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AssetEntityTypeConfiguration());
    }
}