using BuildingBlocks.Domain.Identifiers.Orders;
using System.Linq.Expressions;
using BuildingBlocks.Application;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

/// <summary>A shelf line as the filters and the pair balance need it.</summary>
internal sealed record FabricShelfLine(FabricStockId Id, string FabricCode, string FabricName, string Color, string ColorKey)
{
    public string Pair => $"{FabricAliases.Canonical(FabricCode)}|{ColorKey}";

    /// <summary>The fabric codes (with the canonical one), name and colour, lower case, as the search reads them.</summary>
    public string Text => SearchWords.Normalize(string.Join('\n', FabricCode, FabricAliases.Canonical(FabricCode),
        FabricName, Color).Normalize(System.Text.NormalizationForm.FormC));
}

/// <summary>How many movements of one kind, state and target there are among the filtered ones.</summary>
internal sealed record FabricMovementGroup(FabricMovementKind Kind, bool IsReversed, bool IsVoided, bool HasItem,
    int Count);

/// <summary>
/// «Рух тканини» in SQL (database audit 2026-11-15, 1.4): the shelf lines are few and read whole, the ledger is
/// filtered, counted and paged by the database, and the pair balance reads only the pairs of the page.
/// </summary>
internal static class FabricMovementLedger
{
    public static Task<List<FabricShelfLine>> ShelfAsync(IWarehouseDbContext db, CancellationToken cancellationToken) =>
        db.FabricStocks.AsNoTracking()
            .Select(x => new FabricShelfLine(x.Id, x.FabricCode, x.FabricName, x.Color, x.ColorKey))
            .ToListAsync(cancellationToken);

    /// <summary>Every filter but the kind; a fabric means any code of its group, the colour its key.</summary>
    public static IQueryable<FabricMovement> Filter(IQueryable<FabricMovement> movements, GetFabricMovementsQuery query,
        IReadOnlyCollection<FabricShelfLine> shelf)
    {
        if (!string.IsNullOrWhiteSpace(query.FabricCode) || !string.IsNullOrWhiteSpace(query.Color))
        {
            var codes = string.IsNullOrWhiteSpace(query.FabricCode)
                ? null
                : FabricAliases.Members(query.FabricCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var key = string.IsNullOrWhiteSpace(query.Color) ? null : StockColor.Key(query.Color);
            var lines = shelf.Where(x => (codes is null || codes.Contains(x.FabricCode)) && (key is null || x.ColorKey == key))
                .Select(x => x.Id).ToList();
            movements = movements.Where(x => lines.Contains(x.StockId));
        }

        if (query.ItemId is { } itemId) movements = movements.Where(x => x.ItemId == new ProductionItemId(itemId));
        if (query.From is { } from) movements = movements.Where(x => x.OperationDate >= from);
        if (query.To is { } to) movements = movements.Where(x => x.OperationDate <= to);
        foreach (var word in SearchWords.Words(query.Search)) movements = movements.Where(Matches(word, shelf));
        return movements;
    }

    /// <summary>
    /// A word is found in the movement's supplier, document, model, destination, note or author (the indexed
    /// <c>SearchText</c>), in its order number («ц-14» or «14»), or in the fabric codes, name or colour of its line.
    /// </summary>
    private static Expression<Func<FabricMovement, bool>> Matches(string word, IEnumerable<FabricShelfLine> shelf)
    {
        var pattern = SearchWords.Patterns(word).Single();
        var lines = shelf.Where(x => x.Text.Contains(word, StringComparison.Ordinal)).Select(x => x.Id).ToList();
        return x => EF.Functions.Like(EF.Property<string>(x, SearchWords.Column), pattern, SearchWords.Escape)
                    || lines.Contains(x.StockId)
                    || ("ц-" + x.OrderNumber.ToString() + " " + x.OrderNumber.ToString()).Contains(word);
    }

    /// <summary>The movements of a kind chip, as a database filter.</summary>
    public static Expression<Func<FabricMovement, bool>> Of(FabricMovementFilter kind) => kind switch
    {
        FabricMovementFilter.Receipt => x => x.Kind == FabricMovementKind.Receipt,
        FabricMovementFilter.Issue => x => x.Kind == FabricMovementKind.Issue,
        FabricMovementFilter.Opening => x => x.Kind == FabricMovementKind.Opening && !x.IsReversed && !x.IsVoided,
        FabricMovementFilter.ItemIssue => x => x.Kind == FabricMovementKind.Issue && x.ItemId != null,
        FabricMovementFilter.OneOffIssue => x => x.Kind == FabricMovementKind.Issue && x.ItemId == null,
        FabricMovementFilter.Adjustment => x => x.Kind == FabricMovementKind.Adjustment,
        FabricMovementFilter.Cancelled => x => x.IsVoided || x.IsReversed || x.Kind == FabricMovementKind.Reversal,
        _ => x => true
    };

    /// <summary>The same chips over the counted groups; must agree with <see cref="Of"/>.</summary>
    public static bool Is(FabricMovementGroup x, FabricMovementFilter kind) => kind switch
    {
        FabricMovementFilter.Receipt => x.Kind == FabricMovementKind.Receipt,
        FabricMovementFilter.Issue => x.Kind == FabricMovementKind.Issue,
        FabricMovementFilter.Opening => x.Kind == FabricMovementKind.Opening && !x.IsReversed && !x.IsVoided,
        FabricMovementFilter.ItemIssue => x.Kind == FabricMovementKind.Issue && x.HasItem,
        FabricMovementFilter.OneOffIssue => x.Kind == FabricMovementKind.Issue && !x.HasItem,
        FabricMovementFilter.Adjustment => x.Kind == FabricMovementKind.Adjustment,
        FabricMovementFilter.Cancelled => x.IsVoided || x.IsReversed || x.Kind == FabricMovementKind.Reversal,
        _ => true
    };

    /// <summary>One small query: the filtered movements counted by kind, state and whether they are for an item.</summary>
    public static Task<List<FabricMovementGroup>> GroupsAsync(IQueryable<FabricMovement> movements,
        CancellationToken cancellationToken) =>
        movements.GroupBy(x => new { x.Kind, x.IsReversed, x.IsVoided, HasItem = x.ItemId != null })
            .Select(g => new FabricMovementGroup(g.Key.Kind, g.Key.IsReversed, g.Key.IsVoided, g.Key.HasItem, g.Count()))
            .ToListAsync(cancellationToken);

    public static int Count(IEnumerable<FabricMovementGroup> groups, FabricMovementFilter kind) =>
        groups.Where(x => Is(x, kind)).Sum(x => x.Count);

    public static FabricMovementCountsResponse Counts(IReadOnlyCollection<FabricMovementGroup> groups) => new(
        Count(groups, FabricMovementFilter.All), Count(groups, FabricMovementFilter.Receipt),
        Count(groups, FabricMovementFilter.ItemIssue), Count(groups, FabricMovementFilter.OneOffIssue),
        Count(groups, FabricMovementFilter.Adjustment), Count(groups, FabricMovementFilter.Opening),
        Count(groups, FabricMovementFilter.Cancelled));

    /// <summary>
    /// «Залишок пари» after each listed movement: the running sum of every movement of the merged pair (canonical
    /// fabric and colour key, as on «Залишки») in the ledger order. A voided movement adds nothing. Only the movements
    /// of the page's pairs are read, as four numbers each.
    /// </summary>
    public static async Task<Dictionary<int, decimal>> PairBalancesAsync(IWarehouseDbContext db,
        IReadOnlyCollection<FabricShelfLine> shelf, IReadOnlyCollection<FabricMovement> rows,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0) return [];
        var pairOf = shelf.ToDictionary(x => x.Id, x => x.Pair);
        var wanted = rows.Select(x => pairOf[x.StockId]).ToHashSet();
        var lines = shelf.Where(x => wanted.Contains(x.Pair)).Select(x => x.Id).ToList();
        var ledger = await db.FabricMovements.AsNoTracking().Where(x => lines.Contains(x.StockId))
            .Select(x => new { x.Id, x.StockId, x.OperationDate, x.CreatedAt, x.Delta, x.IsVoided })
            .ToListAsync(cancellationToken);
        var ids = rows.Select(x => x.Id).ToHashSet();
        var result = new Dictionary<int, decimal>();
        foreach (var pair in ledger.GroupBy(x => pairOf[x.StockId]))
        {
            var balance = 0m;
            foreach (var movement in pair.OrderBy(x => x.OperationDate).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id))
            {
                if (!movement.IsVoided) balance += movement.Delta;
                if (ids.Contains(movement.Id)) result[movement.Id.Value] = balance;
            }
        }

        return result;
    }
}
