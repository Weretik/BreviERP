
using BuildingBlocks.Domain.Entity;

namespace Catalog.Application.Abstractions;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot { }
