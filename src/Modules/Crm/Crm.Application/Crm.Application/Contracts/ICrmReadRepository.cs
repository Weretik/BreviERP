using BuildingBlocks.Domain.Abstractions;

namespace Crm.Application.Contracts;

public interface ICrmReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
