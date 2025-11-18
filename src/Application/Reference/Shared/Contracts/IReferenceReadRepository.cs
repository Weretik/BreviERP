namespace Application.Reference.Shared;

public interface IReferenceReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
