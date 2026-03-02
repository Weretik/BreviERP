using Ardalis.Specification;
using BuildingBlocks.Domain.Abstractions;

namespace Accounting.Application.Contracts.Persistence;

public interface IAccountingReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
