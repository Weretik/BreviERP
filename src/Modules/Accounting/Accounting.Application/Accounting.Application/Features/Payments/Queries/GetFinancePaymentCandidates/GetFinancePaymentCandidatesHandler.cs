using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Payments.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Payments.Queries.GetFinancePaymentCandidates;

public sealed class GetFinancePaymentCandidatesHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : IQueryHandler<GetFinancePaymentCandidatesQuery, Result<IReadOnlyList<FinanceTransactionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceTransactionResponse>>> Handle(GetFinancePaymentCandidatesQuery query,
        CancellationToken cancellationToken)
    {
        var rows = db.Transactions.AsNoTracking()
            .Where(x => x.Status == MovementStatus.Active && x.Type == CategoryDirection.Expense);
        var search = FinanceText.Clean(query.Search);
        if (search.Length > 0)
        {
            var term = search.ToLowerInvariant();
            var amount = AmountOf(search);
            rows = rows.Where(x => x.Description.ToLower().Contains(term) || x.Comment.ToLower().Contains(term)
                || x.OneoffName.ToLower().Contains(term) || (amount != null && x.AmountMinor == amount));
        }
        else if (query.SupplierId is { } supplierId)
        {
            // The validator lets only a positive supplier id through, which is always a valid link.
            var link = CounterpartyLink.Of(CounterpartyKind.Supplier, supplierId.ToString()).Value;
            rows = rows.Where(x => x.CounterpartyKind == link.Kind && x.CounterpartyId == link.Id);
        }
        else if (query.AmountMinor is { } exact)
        {
            rows = rows.Where(x => x.AmountMinor == exact);
        }
        else
        {
            return Result.Success<IReadOnlyList<FinanceTransactionResponse>>([]);
        }

        var target = query.AmountMinor ?? -1;
        var page = await rows.OrderByDescending(x => x.AmountMinor == target).ThenByDescending(x => x.Date)
            .ThenByDescending(x => x.Id).Take(GetFinancePaymentCandidatesQuery.Limit).ToListAsync(cancellationToken);
        return Result.Success(await FinancePaymentRows.RespondAsync(db, customers, suppliers, page, cancellationToken));
    }

    /// <summary>«1 250,40» or «1250.4» as kopecks; null when the text is not an amount.</summary>
    public static long? AmountOf(string text)
    {
        var clean = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray()).Replace(',', '.');
        return decimal.TryParse(clean, System.Globalization.NumberStyles.AllowDecimalPoint,
                   System.Globalization.CultureInfo.InvariantCulture, out var value)
               && value > 0 && decimal.Round(value, 2) == value
            ? (long)(value * 100)
            : null;
    }
}
