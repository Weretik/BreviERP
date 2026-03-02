using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Domain.Abstractions;
using Crm.Application.Contracts;
using Crm.Infrastructure.DataBase;

namespace Crm.Infrastructure.Repositories;

public sealed class CrmReadEfRepository<T>(CrmDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICrmReadRepository<T>
    where T : class, IAggregateRoot
{
}
