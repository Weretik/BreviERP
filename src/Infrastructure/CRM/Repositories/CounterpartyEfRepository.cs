using Application.CRM.Shared.Contracts;

namespace Infrastructure.CRM.Repositories;

public sealed class CounterpartyEfRepository<T>(CrmDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICounterpartyRepository<T>
    where T : class, IAggregateRoot { }
