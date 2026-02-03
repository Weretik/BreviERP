using BuildingBlocks.Domain.Abstractions;
using Reference.Application.Contracts.Persistence;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Repositories;

internal sealed class ReferenceEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceRepository<T>
    where T : class, IAggregateRoot { }
