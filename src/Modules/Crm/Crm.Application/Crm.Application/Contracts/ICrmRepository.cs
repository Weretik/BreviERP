using BuildingBlocks.Domain.Abstractions;

namespace Crm.Application.Contracts;

public interface ICrmRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
