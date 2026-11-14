using BuildingBlocks.Domain.Abstractions;
using Catalog.Application.Abstractions;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
