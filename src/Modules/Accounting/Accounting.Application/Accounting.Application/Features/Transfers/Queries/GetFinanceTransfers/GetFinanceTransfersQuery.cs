using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;

/// <summary>
/// Filters of «Перекази»: dates inclusive, <c>AccountId</c> matches either side, <c>Search</c> looks in the note,
/// amounts are kopecks. Without <c>IncludeVoided</c> only active transfers are listed.
/// </summary>
public sealed record FinanceTransferFilter(
    LedgerScope Scope,
    DateOnly? From,
    DateOnly? To,
    Guid? AccountId,
    string? Search,
    long? MinMinor,
    long? MaxMinor,
    MovementSource? Source,
    bool IncludeVoided);

/// <summary>«Перекази»: 50 rows per page, newest first by default, with the total of the whole selection.</summary>
public sealed record GetFinanceTransfersQuery(FinanceTransferFilter Filter, FinanceJournalSort Sort,
    FinanceSortDirection Direction, int Page) : IQuery<Result<FinanceTransferPageResponse>>;

public sealed class GetFinanceTransfersValidator : AbstractValidator<GetFinanceTransfersQuery>
{
    public GetFinanceTransfersValidator()
    {
        RuleFor(x => x.Filter.Scope).IsInEnum();
        RuleFor(x => x.Filter.Source).IsInEnum().When(x => x.Filter.Source is not null);
        RuleFor(x => x.Filter.Search).MaximumLength(FinanceJournal.SearchMaxLength);
        RuleFor(x => x.Filter.To).GreaterThanOrEqualTo(x => x.Filter.From)
            .When(x => x.Filter.From is not null && x.Filter.To is not null)
            .WithMessage("Кінець періоду раніший за початок.");
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Page).GreaterThan(0);
    }
}

public sealed class GetFinanceTransfersHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceTransfersQuery, Result<FinanceTransferPageResponse>>
{
    public async ValueTask<Result<FinanceTransferPageResponse>> Handle(GetFinanceTransfersQuery query,
        CancellationToken cancellationToken)
    {
        var filtered = Filter(db.Transfers.AsNoTracking(), query.Filter);
        var total = await filtered.CountAsync(cancellationToken);
        IReadOnlyList<FinanceTransferTotalResponse> totals = (await filtered.Where(x => x.Status == MovementStatus.Active)
                .GroupBy(x => x.Currency)
                .Select(g => new { Currency = g.Key, Sum = g.Sum(x => x.AmountMinor) })
                .ToListAsync(cancellationToken))
            .OrderBy(x => x.Currency).Select(x => new FinanceTransferTotalResponse(x.Currency, x.Sum)).ToList();

        var asc = query.Direction == FinanceSortDirection.Asc;
        var ordered = query.Sort switch
        {
            FinanceJournalSort.Amount => asc
                ? filtered.OrderBy(x => x.AmountMinor).ThenByDescending(x => x.Date).ThenByDescending(x => x.Id)
                : filtered.OrderByDescending(x => x.AmountMinor).ThenByDescending(x => x.Date).ThenByDescending(x => x.Id),
            _ => asc
                ? filtered.OrderBy(x => x.Date).ThenBy(x => x.Id)
                : filtered.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id),
        };
        var page = await ordered.Skip((query.Page - 1) * FinanceJournal.PageSize).Take(FinanceJournal.PageSize)
            .ToListAsync(cancellationToken);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        return Result.Success(new FinanceTransferPageResponse(
            page.Select(x => FinanceTransferResponse.From(x, names)).ToList(), total, query.Page, FinanceJournal.PageSize,
            totals));
    }

    private static IQueryable<FinanceTransfer> Filter(IQueryable<FinanceTransfer> rows, FinanceTransferFilter f)
    {
        rows = rows.Where(x => x.Scope == f.Scope);
        if (!f.IncludeVoided) rows = rows.Where(x => x.Status == MovementStatus.Active);
        if (f.From is { } from) rows = rows.Where(x => x.Date >= from);
        if (f.To is { } to) rows = rows.Where(x => x.Date <= to);
        if (f.AccountId is { } account)
        {
            var id = FinanceAccountId.From(account);
            rows = rows.Where(x => x.FromAccountId == id || x.ToAccountId == id);
        }

        if (f.MinMinor is { } min) rows = rows.Where(x => x.AmountMinor >= min);
        if (f.MaxMinor is { } max) rows = rows.Where(x => x.AmountMinor <= max);
        if (f.Source is { } source) rows = rows.Where(x => x.Source == source);
        var search = FinanceText.Clean(f.Search).ToLowerInvariant();
        if (search.Length > 0) rows = rows.Where(x => x.Note.ToLower().Contains(search));
        return rows;
    }
}
