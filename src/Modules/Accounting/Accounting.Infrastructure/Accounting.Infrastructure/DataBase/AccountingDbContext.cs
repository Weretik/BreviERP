using Accounting.Application.Contracts.Persistence;
using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure.DataBase;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options), IReadAccountingDbContext
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
