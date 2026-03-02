using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Contracts.Persistence;

public interface IReadAccountingDbContext
{
    DbSet<TransactionCategory> TransactionCategories { get; }

}
