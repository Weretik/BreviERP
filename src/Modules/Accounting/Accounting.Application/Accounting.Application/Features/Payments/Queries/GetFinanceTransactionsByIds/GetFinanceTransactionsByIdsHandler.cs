using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Payments.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Payments.Queries.GetFinanceTransactionsByIds;

public sealed class GetFinanceTransactionsByIdsHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : IQueryHandler<GetFinanceTransactionsByIdsQuery, Result<IReadOnlyList<FinanceTransactionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceTransactionResponse>>> Handle(GetFinanceTransactionsByIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Ids.Count == 0) return Result.Success<IReadOnlyList<FinanceTransactionResponse>>([]);
        var ids = query.Ids.Distinct().Select(FinanceTransactionId.From).ToList();
        var rows = await db.Transactions.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        return Result.Success(await FinancePaymentRows.RespondAsync(db, customers, suppliers, rows, cancellationToken));
    }
}
