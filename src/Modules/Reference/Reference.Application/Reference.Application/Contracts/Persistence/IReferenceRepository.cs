using BuildingBlocks.Domain.Abstractions;

namespace Reference.Application.Contracts.Persistence;

public interface IReferenceRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }

