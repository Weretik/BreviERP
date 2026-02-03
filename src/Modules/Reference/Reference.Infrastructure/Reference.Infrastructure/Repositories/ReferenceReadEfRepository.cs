using BuildingBlocks.Domain.Abstractions;
using Reference.Application.Contracts.Persistence;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Repositories;

internal sealed class ReferenceReadEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceReadRepository<T>
    where T : class, IAggregateRoot { }
