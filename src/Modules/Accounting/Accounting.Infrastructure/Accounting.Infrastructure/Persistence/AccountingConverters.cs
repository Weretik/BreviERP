using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Accounting.Infrastructure.Persistence;

internal static class AccountingConverters
{
    public static readonly ValueConverter<FinanceAccountId, Guid> AccountId = new(id => id.Value, v => FinanceAccountId.From(v));
    public static readonly ValueConverter<FinanceCategoryId, Guid> CategoryId = new(id => id.Value, v => FinanceCategoryId.From(v));
}
