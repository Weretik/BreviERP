using Reference.Application.Abstractions;
using BuildingBlocks.Domain.Entity;

namespace Reference.Infrastructure.Persistence.Repositories;

internal sealed class ReferenceReadEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceReadRepository<T>
    where T : class, IAggregateRoot { }
