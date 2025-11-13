using Application.Reference.Shared;

namespace Infrastructure.Reference.Repositories;

internal sealed class ReferenceEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceRepository<T>
    where T : class, IAggregateRoot { }
