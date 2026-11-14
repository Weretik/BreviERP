using BuildingBlocks.Domain.Abstractions;

namespace Reference.Application.Abstractions;

public interface IReferenceReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
