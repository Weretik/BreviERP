using Catalog.Application.Abstractions;
using BuildingBlocks.Domain.Entity;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CatalogReadEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogReadRepository<T>
    where T : class, IAggregateRoot { }
