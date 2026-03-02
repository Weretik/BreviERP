using Accounting.Application.Contracts.Persistence;
using Accounting.Infrastructure.DataBase;
using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Domain.Abstractions;

namespace Accounting.Infrastructure.Repositories;

public sealed class AccountingReadEfRepository<T>(AccountingDbContext dbContext)
    : RepositoryBase<T>(dbContext), IAccountingReadRepository<T>
    where T : class, IAggregateRoot
{
}
