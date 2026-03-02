using Crm.Domain.Entities;

namespace Crm.Application.Contracts;

public interface IReadCrmDbContext
{
    DbSet<Counterparty> Counterparty { get; }
}
