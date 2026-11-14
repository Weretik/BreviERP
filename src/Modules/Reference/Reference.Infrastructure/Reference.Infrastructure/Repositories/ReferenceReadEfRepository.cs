using BuildingBlocks.Domain.Abstractions;
using Reference.Infrastructure.DataBase;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Repositories;

internal sealed class ReferenceReadEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceReadRepository<T>
    where T : class, IAggregateRoot { }
