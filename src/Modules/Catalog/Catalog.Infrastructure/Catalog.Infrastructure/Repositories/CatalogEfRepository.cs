using BuildingBlocks.Domain.Abstractions;
using Catalog.Infrastructure.DataBase;
using Catalog.Application.Abstractions;

namespace Catalog.Infrastructure.Repositories;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
