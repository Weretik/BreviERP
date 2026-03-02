using Ardalis.Specification;
using BuildingBlocks.Domain.Abstractions;

namespace Accounting.Application.Contracts.Persistence;

public interface IAccountingRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
