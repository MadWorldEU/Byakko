using MadWorldEU.Byakko.Accounts;
using MadWorldEU.Byakko.Audits;

namespace MadWorldEU.Byakko;

public sealed class ByakkoContext(DbContextOptions<ByakkoContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AssetEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AccountEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AuditEntityTypeConfiguration());
    }
}