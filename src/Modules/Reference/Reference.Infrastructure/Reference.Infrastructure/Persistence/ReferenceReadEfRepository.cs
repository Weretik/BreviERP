using BuildingBlocks.Domain.Abstractions;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Persistence;

internal sealed class ReferenceReadEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceReadRepository<T>
    where T : class, IAggregateRoot { }
