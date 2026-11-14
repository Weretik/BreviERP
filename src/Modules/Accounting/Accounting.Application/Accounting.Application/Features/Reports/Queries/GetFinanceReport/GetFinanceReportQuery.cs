using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

/// <summary>
/// The settings of a report of «Звіти». Dates are inclusive; <c>Scope</c> null takes both areas; <c>CategoryId</c> takes a
/// root with its subcategories (or one subcategory). Only active income and expense are counted. The accounts report
/// adds the transfers of the period, area and account; the type, category and counterparty do not filter them.
/// </summary>
public sealed record FinanceReportFilter(
    FinanceReportKind Kind,
    DateOnly From,
    DateOnly To,
    LedgerScope? Scope = null,
    Guid? AccountId = null,
    CategoryDirection? Type = null,
    Guid? CategoryId = null,
    CounterpartyLink? Counterparty = null);

/// <summary>Builds a report; the operations report keeps at most <paramref name="RowLimit"/> rows.</summary>
public sealed record GetFinanceReportQuery(FinanceReportFilter Filter, int RowLimit = FinanceReports.ScreenRows)
    : IQuery<Result<FinanceReportResponse>>;

public static class FinanceReports
{
    /// <summary>Operations shown on the screen.</summary>
    public const int ScreenRows = 100;

    /// <summary>Operations written to a CSV file; a longer journal is split by dates.</summary>
    public const int CsvRows = 50_000;

    public const string WithoutCategory = "Без категорії";
    public const string WithoutSubcategory = "Без підкатегорії";
    public const string WithoutCounterparty = "Без контрагента";
    public const string UnknownCounterparty = "Контрагента не знайдено";
}

public sealed class GetFinanceReportValidator : AbstractValidator<GetFinanceReportQuery>
{
    public GetFinanceReportValidator()
    {
        RuleFor(x => x.Filter.Kind).IsInEnum();
        RuleFor(x => x.Filter.Scope).IsInEnum().When(x => x.Filter.Scope is not null);
        RuleFor(x => x.Filter.Type).IsInEnum().When(x => x.Filter.Type is not null);
        RuleFor(x => x.Filter.To).GreaterThanOrEqualTo(x => x.Filter.From)
            .WithMessage("Кінець періоду раніший за початок.");
        RuleFor(x => x.RowLimit).InclusiveBetween(1, FinanceReports.CsvRows);
    }
}

public sealed class GetFinanceReportHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers) : IQueryHandler<GetFinanceReportQuery, Result<FinanceReportResponse>>
{
    public async ValueTask<Result<FinanceReportResponse>> Handle(GetFinanceReportQuery query,
        CancellationToken cancellationToken)
    {
        var f = query.Filter;
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var operations = Filter(db.Transactions.AsNoTracking(), f, names);

        var sums = await operations.GroupBy(x => new { x.Currency, x.Type })
            .Select(g => new { g.Key.Currency, g.Key.Type, Sum = g.Sum(x => x.AmountMinor), Count = g.Count() })
            .ToListAsync(cancellationToken);
        IReadOnlyList<FinanceReportTotalResponse> totals = sums.GroupBy(x => x.Currency).OrderBy(g => Currency(g.Key))
            .Select(g =>
            {
                var income = g.Where(x => x.Type == CategoryDirection.Income).Sum(x => x.Sum);
                var expense = g.Where(x => x.Type == CategoryDirection.Expense).Sum(x => x.Sum);
                return new FinanceReportTotalResponse(g.Key, g.Sum(x => x.Count), income, expense, income - expense);
            }).ToList();

        var (rows, total) = f.Kind switch
        {
            FinanceReportKind.Summary => All(totals.Select(x => new FinanceReportRowResponse(FinanceReportRowType.Group,
                x.Currency, x.Operations, x.IncomeMinor, x.ExpenseMinor, x.ResultMinor)).ToList()),
            FinanceReportKind.ExpenseStructure => All(await ExpenseStructureAsync(operations, names, cancellationToken)),
            FinanceReportKind.Categories => All(await CategoriesAsync(operations, names, cancellationToken)),
            FinanceReportKind.Accounts => All(await AccountsAsync(operations, f, cancellationToken)),
            FinanceReportKind.Counterparties => All(await CounterpartiesAsync(operations, cancellationToken)),
            _ => await OperationsAsync(operations, names, query.RowLimit, cancellationToken),
        };
        return Result.Success(new FinanceReportResponse(f.Kind, f.From, f.To, totals, rows, total,
            total > rows.Count));
    }

    private static (IReadOnlyList<FinanceReportRowResponse> Rows, int Total) All(IReadOnlyList<FinanceReportRowResponse> rows) =>
        (rows, rows.Count);

    private static IQueryable<FinanceTransaction> Filter(IQueryable<FinanceTransaction> rows, FinanceReportFilter f,
        FinanceNames names)
    {
        rows = rows.Where(x => x.Status == MovementStatus.Active && x.Date >= f.From && x.Date <= f.To);
        if (f.Scope is { } scope) rows = rows.Where(x => x.Scope == scope);
        if (f.AccountId is { } account)
        {
            var id = FinanceAccountId.From(account);
            rows = rows.Where(x => x.AccountId == id);
        }

        var type = f.Kind == FinanceReportKind.ExpenseStructure ? CategoryDirection.Expense : f.Type;
        if (type is { } direction) rows = rows.Where(x => x.Type == direction);
        if (f.CategoryId is { } category)
        {
            var ids = names.Branch(FinanceCategoryId.From(category));
            rows = rows.Where(x => ids.Contains(x.CategoryId));
        }

        if (f.Counterparty is { } link)
            rows = rows.Where(x => x.CounterpartyKind == link.Kind && x.CounterpartyId == link.Id);
        return rows;
    }

    private static async Task<IReadOnlyList<FinanceReportRowResponse>> ExpenseStructureAsync(
        IQueryable<FinanceTransaction> operations, FinanceNames names, CancellationToken cancellationToken)
    {
        var groups = await operations.GroupBy(x => new { x.Currency, x.CategoryId })
            .Select(g => new { g.Key.Currency, g.Key.CategoryId, Sum = g.Sum(x => x.AmountMinor), Count = g.Count() })
            .ToListAsync(cancellationToken);
        var rows = new List<FinanceReportRowResponse>();
        foreach (var currency in groups.GroupBy(x => x.Currency).OrderBy(g => Currency(g.Key)))
        {
            var placed = currency.Select(x =>
            {
                var category = names.Category(x.CategoryId);
                var root = names.Root(category);
                return (Root: root, Sub: category is not null && category != root ? category : null, x.Sum, x.Count);
            }).ToList();
            foreach (var root in placed.GroupBy(x => x.Root?.Id).Select(g => g.ToList())
                         .OrderByDescending(g => g.Sum(x => x.Sum)).ThenBy(g => g[0].Root?.Name))
            {
                var name = root[0].Root?.Name ?? FinanceReports.WithoutCategory;
                var sum = root.Sum(x => x.Sum);
                rows.Add(new FinanceReportRowResponse(FinanceReportRowType.Group, currency.Key, root.Sum(x => x.Count), 0,
                    sum, -sum, Category: name));
                if (root.All(x => x.Sub is null)) continue;
                foreach (var sub in root.GroupBy(x => x.Sub?.Id).Select(g => g.ToList())
                             .OrderBy(g => g[0].Sub is null ? 1 : 0).ThenByDescending(g => g.Sum(x => x.Sum))
                             .ThenBy(g => g[0].Sub?.Name))
                {
                    var subSum = sub.Sum(x => x.Sum);
                    rows.Add(new FinanceReportRowResponse(FinanceReportRowType.Subcategory, currency.Key,
                        sub.Sum(x => x.Count), 0, subSum, -subSum, Category: name,
                        Subcategory: sub[0].Sub?.Name ?? FinanceReports.WithoutSubcategory));
                }
            }
        }

        return rows;
    }

    private static async Task<IReadOnlyList<FinanceReportRowResponse>> CategoriesAsync(
        IQueryable<FinanceTransaction> operations, FinanceNames names, CancellationToken cancellationToken)
    {
        var groups = await operations.GroupBy(x => new { x.Currency, x.Type, x.CategoryId })
            .Select(g => new { g.Key.Currency, g.Key.Type, g.Key.CategoryId, Sum = g.Sum(x => x.AmountMinor), Count = g.Count() })
            .ToListAsync(cancellationToken);
        return groups.Select(x =>
            {
                var category = names.Category(x.CategoryId);
                var root = names.Root(category);
                return Row(FinanceReportRowType.Group, x.Currency, x.Type, x.Sum, x.Count) with
                {
                    Type = x.Type,
                    Category = root?.Name ?? FinanceReports.WithoutCategory,
                    Subcategory = category is not null && category != root ? category.Name : null,
                };
            })
            .OrderBy(x => Currency(x.Currency)).ThenBy(x => x.Type == CategoryDirection.Income ? 0 : 1)
            .ThenBy(x => x.Category == FinanceReports.WithoutCategory ? 1 : 0).ThenBy(x => x.Category)
            .ThenBy(x => x.Subcategory is null ? 0 : 1).ThenBy(x => x.Subcategory)
            .ToList();
    }

    private async Task<IReadOnlyList<FinanceReportRowResponse>> AccountsAsync(IQueryable<FinanceTransaction> operations,
        FinanceReportFilter f, CancellationToken cancellationToken)
    {
        var movement = await operations.GroupBy(x => new { x.AccountId, x.Currency, x.Type })
            .Select(g => new { g.Key.AccountId, g.Key.Currency, g.Key.Type, Sum = g.Sum(x => x.AmountMinor), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var transfers = db.Transfers.AsNoTracking()
            .Where(x => x.Status == MovementStatus.Active && x.Date >= f.From && x.Date <= f.To);
        if (f.Scope is { } scope) transfers = transfers.Where(x => x.Scope == scope);
        FinanceAccountId? only = f.AccountId is { } account ? FinanceAccountId.From(account) : null;
        if (only is { } id) transfers = transfers.Where(x => x.FromAccountId == id || x.ToAccountId == id);
        var outgoing = await transfers.GroupBy(x => new { x.FromAccountId, x.Currency })
            .Select(g => new { AccountId = g.Key.FromAccountId, g.Key.Currency, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);
        var incoming = await transfers.GroupBy(x => new { x.ToAccountId, x.Currency })
            .Select(g => new { AccountId = g.Key.ToAccountId, g.Key.Currency, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);

        var accounts = await db.Accounts.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
        var keys = movement.Select(x => (x.AccountId, x.Currency))
            .Concat(outgoing.Select(x => (x.AccountId, x.Currency)))
            .Concat(incoming.Select(x => (x.AccountId, x.Currency)))
            .Where(x => only is null || x.AccountId == only)
            .Distinct();
        return keys.Select(key =>
            {
                var own = movement.Where(x => x.AccountId == key.AccountId && x.Currency == key.Currency).ToList();
                var income = own.Where(x => x.Type == CategoryDirection.Income).Sum(x => x.Sum);
                var expense = own.Where(x => x.Type == CategoryDirection.Expense).Sum(x => x.Sum);
                var into = incoming.Where(x => x.AccountId == key.AccountId && x.Currency == key.Currency).Sum(x => x.Sum);
                var away = outgoing.Where(x => x.AccountId == key.AccountId && x.Currency == key.Currency).Sum(x => x.Sum);
                var account = accounts.GetValueOrDefault(key.AccountId);
                return (Order: account?.SortOrder ?? int.MaxValue, Row: new FinanceReportRowResponse(
                    FinanceReportRowType.Group, key.Currency, own.Sum(x => x.Count), income, expense,
                    income - expense + into - away, Account: account?.Name ?? string.Empty,
                    IncomingTransfersMinor: into, OutgoingTransfersMinor: away));
            })
            .OrderBy(x => x.Order).ThenBy(x => x.Row.Account).ThenBy(x => Currency(x.Row.Currency))
            .Select(x => x.Row).ToList();
    }

    private async Task<IReadOnlyList<FinanceReportRowResponse>> CounterpartiesAsync(
        IQueryable<FinanceTransaction> operations, CancellationToken cancellationToken)
    {
        var groups = await operations.GroupBy(x => new { x.Currency, x.Type, x.CounterpartyKind, x.CounterpartyId, x.OneoffName })
            .Select(g => new
            {
                g.Key.Currency, g.Key.Type, g.Key.CounterpartyKind, g.Key.CounterpartyId, g.Key.OneoffName,
                Sum = g.Sum(x => x.AmountMinor), Count = g.Count(),
            })
            .ToListAsync(cancellationToken);
        var states = await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            groups.Select(x => Link(x.CounterpartyKind, x.CounterpartyId)), cancellationToken);
        return groups
            .GroupBy(x => (x.Currency, Name: CounterpartyName(states, Link(x.CounterpartyKind, x.CounterpartyId), x.OneoffName)))
            .Select(g =>
            {
                var income = g.Where(x => x.Type == CategoryDirection.Income).Sum(x => x.Sum);
                var expense = g.Where(x => x.Type == CategoryDirection.Expense).Sum(x => x.Sum);
                return new FinanceReportRowResponse(FinanceReportRowType.Group, g.Key.Currency, g.Sum(x => x.Count),
                    income, expense, income - expense, Counterparty: g.Key.Name);
            })
            .OrderBy(x => Currency(x.Currency))
            .ThenBy(x => x.Counterparty is FinanceReports.WithoutCounterparty or FinanceReports.UnknownCounterparty ? 1 : 0)
            .ThenBy(x => x.Counterparty)
            .ToList();
    }

    private async Task<(IReadOnlyList<FinanceReportRowResponse> Rows, int Total)> OperationsAsync(
        IQueryable<FinanceTransaction> operations, FinanceNames names, int limit, CancellationToken cancellationToken)
    {
        var total = await operations.CountAsync(cancellationToken);
        var page = await operations.OrderBy(x => x.Date).ThenBy(x => x.Id).Take(limit).ToListAsync(cancellationToken);
        var states = await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            page.Select(x => x.Counterparty), cancellationToken);
        IReadOnlyList<FinanceReportRowResponse> rows = page.Select(x =>
        {
            var category = names.Category(x.CategoryId);
            var root = names.Root(category);
            return Row(FinanceReportRowType.Operation, x.Currency, x.Type, x.AmountMinor, 1) with
            {
                Date = x.Date,
                Account = names.Account(x.AccountId),
                Type = x.Type,
                Category = root?.Name ?? FinanceReports.WithoutCategory,
                Subcategory = category is not null && category != root ? category.Name : null,
                Counterparty = CounterpartyName(states, x.Counterparty, x.OneoffName),
                AmountMinor = x.AmountMinor,
                Note = string.Join(" · ", new[] { x.Description, x.Comment }.Where(s => s.Length > 0)),
                Source = x.Source,
                Id = x.Id.Value,
            };
        }).ToList();
        return (rows, total);
    }

    private static FinanceReportRowResponse Row(FinanceReportRowType rowType, string currency, CategoryDirection type,
        long sum, int count) => type == CategoryDirection.Income
        ? new FinanceReportRowResponse(rowType, currency, count, sum, 0, sum)
        : new FinanceReportRowResponse(rowType, currency, count, 0, sum, -sum);

    private static CounterpartyLink? Link(CounterpartyKind? kind, string? id) =>
        kind is { } k && id is not null ? new CounterpartyLink(k, id) : null;

    /// <summary>The counterparty's name; a one-off name is marked; a link to a removed record is shown as such.</summary>
    private static string CounterpartyName(CounterpartyStates states, CounterpartyLink? link, string oneoff) =>
        link is not null
            ? states.Of(link)?.Name ?? FinanceReports.UnknownCounterparty
            : oneoff.Length > 0 ? $"{oneoff} (разовий)" : FinanceReports.WithoutCounterparty;

    /// <summary>Hryvnia first, then other currencies alphabetically.</summary>
    private static string Currency(string code) => code == "UAH" ? string.Empty : code;
}
