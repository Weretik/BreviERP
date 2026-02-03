using Reference.Application.Contracts.Persistence;
using Reference.Domain.Entities;

namespace Reference.Infrastructure.DataBase;

public class ReferenceDbContext(DbContextOptions<ReferenceDbContext> options)
    : DbContext(options), IReadReferenceDbContext
{
    public DbSet<AdditionalReference> AdditionalReferences => Set<AdditionalReference>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();
    public void DiscardChanges() => ChangeTracker.Clear();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReferenceDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.Reference") ?? false);
    }
}
