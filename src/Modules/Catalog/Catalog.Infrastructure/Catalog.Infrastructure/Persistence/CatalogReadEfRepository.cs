using BuildingBlocks.Domain.Abstractions;
using Catalog.Application.Abstractions;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogReadEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogReadRepository<T>
    where T : class, IAggregateRoot { }
