namespace Application.CRM.Shared.Contracts;

public interface ICrmRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }
