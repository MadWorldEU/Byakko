using Microsoft.EntityFrameworkCore;

namespace MadWorldEU.Byakko.Infrastructure.Postgresql;

public sealed class ByakkoContext(DbContextOptions<ByakkoContext> options) : DbContext(options)
{
    
}