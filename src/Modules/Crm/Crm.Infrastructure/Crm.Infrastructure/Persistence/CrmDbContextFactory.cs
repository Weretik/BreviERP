using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Crm.Infrastructure.Persistence;

public sealed class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<CrmDbContext>()
        .UseNpgsql("Host=localhost;Database=crm_design_only;Username=design", x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")).Options);
}
