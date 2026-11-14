using Catalog.Application.Abstractions;
using BuildingBlocks.Domain.Entity;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
