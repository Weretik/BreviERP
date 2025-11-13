namespace Application.Reference.Shared;

public interface IReferenceRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }

