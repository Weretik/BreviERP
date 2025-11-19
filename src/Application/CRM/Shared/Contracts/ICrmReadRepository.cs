namespace Application.CRM.Shared.Contracts;

public interface ICrmReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot {}
