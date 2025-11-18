using Infrastructure.Reference;

namespace Infrastructure.Common.Events;

public sealed class EfDomainEventContext(ReferenceDbContext db) : IDomainEventContext
{
    public IReadOnlyList<IHasDomainEvents> GetDomainEntities()
        => db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

    public void ClearDomainEvents()
    {
        var entities = db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity);

        foreach (var entity in entities)
            entity.ClearDomainEvents();
    }
}
