using BuildingBlocks.Domain.Abstractions;

namespace Reference.Application.Contracts.Persistence;

public interface IReferenceReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
