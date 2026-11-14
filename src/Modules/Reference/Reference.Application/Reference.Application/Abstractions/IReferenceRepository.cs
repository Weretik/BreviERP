
using BuildingBlocks.Domain.Entity;

namespace Reference.Application.Abstractions;

public interface IReferenceRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }

