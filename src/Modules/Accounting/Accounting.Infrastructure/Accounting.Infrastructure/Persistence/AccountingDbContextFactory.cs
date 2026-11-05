using Microsoft.EntityFrameworkCore.Design;

namespace Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AccountingDbContext>()
        .UseNpgsql("Host=localhost;Database=accounting_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", AccountingDbContext.Schema)).Options);
}
