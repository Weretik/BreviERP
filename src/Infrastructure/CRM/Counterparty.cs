using Domain.CRM.Entities;

namespace Infrastructure.CRM;

public class CounterpartyDbContext(DbContextOptions<CounterpartyDbContext> options) : DbContext(options)
{
    public DbSet<Counterparty> Counterparty => Set<Counterparty>();
    public void DiscardChanges() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CounterpartyDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.CRM") ?? false);
    }
}
