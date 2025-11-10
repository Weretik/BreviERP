using Domain.Accounting.Entities;

namespace Infrastructure.Accounting;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options)
{
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("ltree");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AccountingDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.Accounting") ?? false);
    }
}
