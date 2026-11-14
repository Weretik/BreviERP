using BuildingBlocks.Domain.Abstractions;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Persistence;

internal sealed class ReferenceEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceRepository<T>
    where T : class, IAggregateRoot { }
