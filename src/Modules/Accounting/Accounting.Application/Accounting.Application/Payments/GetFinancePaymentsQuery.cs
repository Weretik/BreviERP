using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Lookup;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Payments;

/// <summary>
/// Operations by id for the payment links of other modules («Рахунки на закупку»): voided ones too, so a link shows
/// what happened; unknown ids are left out.
/// </summary>
public sealed record GetFinanceTransactionsByIdsQuery(IReadOnlyCollection<Guid> Ids)
    : IQuery<Result<IReadOnlyList<FinanceTransactionResponse>>>;

/// <summary>
/// Expenses that may pay a purchase invoice: active, of both scopes. Without <c>Search</c> — the expenses of
/// <c>SupplierId</c>; with it — any expense whose description, note or one-off name contains it, or whose amount equals it
/// in hryvnias. The expenses with <c>AmountMinor</c> come first, then the newest; at most <see cref="Limit"/>.
/// </summary>
public sealed record GetFinancePaymentCandidatesQuery(int? SupplierId, long? AmountMinor, string? Search)
    : IQuery<Result<IReadOnlyList<FinanceTransactionResponse>>>
{
    public const int Limit = 20;
    public const int SearchMaxLength = 200;
}

public sealed class GetFinancePaymentCandidatesQueryValidator : AbstractValidator<GetFinancePaymentCandidatesQuery>
{
    public GetFinancePaymentCandidatesQueryValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.AmountMinor).GreaterThan(0).When(x => x.AmountMinor is not null);
        RuleFor(x => x.Search).MaximumLength(GetFinancePaymentCandidatesQuery.SearchMaxLength);
    }
}

public sealed class FinancePaymentQueriesHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : IQueryHandler<GetFinanceTransactionsByIdsQuery, Result<IReadOnlyList<FinanceTransactionResponse>>>,
        IQueryHandler<GetFinancePaymentCandidatesQuery, Result<IReadOnlyList<FinanceTransactionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceTransactionResponse>>> Handle(GetFinanceTransactionsByIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Ids.Count == 0) return Result.Success<IReadOnlyList<FinanceTransactionResponse>>([]);
        var ids = query.Ids.Distinct().Select(FinanceTransactionId.From).ToList();
        var rows = await db.Transactions.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        return Result.Success(await RespondAsync(rows, cancellationToken));
    }

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
            var link = CounterpartyLink.Of(CounterpartyKind.Supplier, supplierId.ToString());
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
        return Result.Success(await RespondAsync(page, cancellationToken));
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

    private async Task<IReadOnlyList<FinanceTransactionResponse>> RespondAsync(IReadOnlyList<FinanceTransaction> rows,
        CancellationToken cancellationToken)
    {
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        names.With(await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            rows.Select(x => x.Counterparty), cancellationToken));
        return rows.Select(x => FinanceTransactionResponse.From(x, names)).ToList();
    }
}

public sealed class GetFinanceTransactionsByIdsQueryValidator : AbstractValidator<GetFinanceTransactionsByIdsQuery>
{
    public GetFinanceTransactionsByIdsQueryValidator()
    {
        RuleFor(x => x.Ids).NotNull();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
