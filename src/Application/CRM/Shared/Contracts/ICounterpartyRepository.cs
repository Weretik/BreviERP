namespace Application.CRM.Shared.Contracts;

public interface ICounterpartyRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }
