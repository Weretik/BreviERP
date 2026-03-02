using Crm.Application.Contracts;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.DataBase;

public class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options), IReadCrmDbContext
{
    public DbSet<Counterparty> Counterparty => Set<Counterparty>();
    public void DiscardChanges() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CrmDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.CRM") ?? false);
    }
}
