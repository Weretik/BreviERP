using Application.CRM.Shared.Contracts;

namespace Infrastructure.CRM.Repositories;

public sealed class CrmEfRepository<T>(CrmDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICrmRepository<T>
    where T : class, IAggregateRoot { }
