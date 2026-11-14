
using BuildingBlocks.Domain.Entity;

namespace Catalog.Application.Abstractions;

public interface ICatalogRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }
