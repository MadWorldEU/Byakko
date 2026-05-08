namespace MadWorldEU.Byakko;

public sealed class ByakkoContext(DbContextOptions<ByakkoContext> options) : DbContext(options)
{
}