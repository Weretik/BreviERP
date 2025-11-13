using Domain.Reference.Entities;

namespace Infrastructure.Reference;

public class ReferenceDbContext(DbContextOptions<ReferenceDbContext> options) : DbContext(options)
{
    public DbSet<AdditionalReference> AdditionalReferences => Set<AdditionalReference>();
    public void DiscardChanges() => ChangeTracker.Clear();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReferenceDbContext).Assembly,
            type => type.Namespace?.StartsWith("Infrastructure.Reference") ?? false);
    }
}
