
using BuildingBlocks.Domain.Entity;

namespace Reference.Application.Abstractions;

public interface IReferenceReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
