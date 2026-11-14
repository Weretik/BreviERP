using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Payments.Shared;

/// <summary>Operations of «Бухгалтерія» as payment rows, with names and counterparties.</summary>
internal static class FinancePaymentRows
{
    public static async Task<IReadOnlyList<FinanceTransactionResponse>> RespondAsync(IAccountingDbContext db,
        IAccountingCustomerReader customers, IAccountingSupplierReader suppliers, IReadOnlyList<FinanceTransaction> rows,
        CancellationToken cancellationToken)
    {
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        names.With(await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            rows.Select(x => x.Counterparty), cancellationToken));
        return rows.Select(x => FinanceTransactionResponse.From(x, names)).ToList();
    }
}
