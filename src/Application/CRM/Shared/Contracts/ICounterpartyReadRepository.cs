namespace Application.CRM.Shared.Contracts;

public interface ICounterpartyReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
