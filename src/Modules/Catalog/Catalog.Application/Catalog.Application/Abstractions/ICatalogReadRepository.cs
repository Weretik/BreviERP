using BuildingBlocks.Domain.Abstractions;

namespace Catalog.Application.Abstractions;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot { }
