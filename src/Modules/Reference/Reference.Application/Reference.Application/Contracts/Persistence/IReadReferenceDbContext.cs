using Reference.Domain.Entities;

namespace Reference.Application.Contracts.Persistence;

public interface IReadReferenceDbContext
{
    DbSet<AdditionalReference> AdditionalReferences { get; }
    DbSet<Fabric> Fabrics { get; }
}
