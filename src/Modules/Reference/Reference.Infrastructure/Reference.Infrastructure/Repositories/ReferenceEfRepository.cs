using BuildingBlocks.Domain.Abstractions;
using Reference.Infrastructure.DataBase;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Repositories;

internal sealed class ReferenceEfRepository<T>(ReferenceDbContext dbContext)
    : RepositoryBase<T>(dbContext), IReferenceRepository<T>
    where T : class, IAggregateRoot { }
