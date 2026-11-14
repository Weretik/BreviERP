using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

public sealed class GetFabricMovementsHandler(IWarehouseDbContext db)
    : IQueryHandler<GetFabricMovementsQuery, Result<FabricMovementPageResponse>>
{
    public const int PageSize = 50;

    public async ValueTask<Result<FabricMovementPageResponse>> Handle(GetFabricMovementsQuery query,
        CancellationToken cancellationToken)
    {
        // The ledger is a few thousand rows: it is read whole, so the pair balance and the counts see every movement.
        var all = await db.FabricMovements.AsNoTracking().Include(x => x.Stock).ToListAsync(cancellationToken);
        var codes = string.IsNullOrWhiteSpace(query.FabricCode)
            ? null
            : FabricAliases.Members(query.FabricCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var key = string.IsNullOrWhiteSpace(query.Color) ? null : StockColor.Key(query.Color);
        var words = Words(query.Search);
        var matching = all.Where(x =>
                (codes is null || codes.Contains(x.Stock.FabricCode)) &&
                (key is null || StockColor.Key(x.Stock.Color) == key) &&
                (query.ItemId is not { } itemId || x.ItemId == itemId) &&
                (query.From is not { } from || x.OperationDate >= from) &&
                (query.To is not { } to || x.OperationDate <= to) &&
                (words.Length == 0 || Matches(x, words)))
            .ToList();
        var counts = new FabricMovementCountsResponse(matching.Count,
            matching.Count(x => Is(x, FabricMovementFilter.Receipt)),
            matching.Count(x => Is(x, FabricMovementFilter.ItemIssue)),
            matching.Count(x => Is(x, FabricMovementFilter.OneOffIssue)),
            matching.Count(x => Is(x, FabricMovementFilter.Adjustment)),
            matching.Count(x => Is(x, FabricMovementFilter.Opening)),
            matching.Count(x => Is(x, FabricMovementFilter.Cancelled)));
        var listed = matching.Where(x => Is(x, query.Kind))
            .OrderByDescending(x => x.OperationDate).ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .ToList();
        var rows = listed.Skip((query.Page - 1) * PageSize).Take(PageSize).ToList();

        var products = await ProductsAsync(rows, cancellationToken);
        var pairs = PairBalances(all, rows);
        var page = rows
            .Select(x => Map(x, query.Actor, x.IssueLineId is { } line ? products.GetValueOrDefault(line) : null,
                pairs.GetValueOrDefault(x.Id)))
            .ToList();
        return Result.Success(new FabricMovementPageResponse(page, query.Page, listed.Count > query.Page * PageSize,
            StockList.Permissions(query.Actor), listed.Count, counts));
    }

    /// <summary>Whether a movement belongs to a kind chip of «Рух тканини».</summary>
    public static bool Is(FabricMovement x, FabricMovementFilter kind) => kind switch
    {
        FabricMovementFilter.Receipt => x.Kind == FabricMovementKind.Receipt,
        FabricMovementFilter.Issue => x.Kind == FabricMovementKind.Issue,
        FabricMovementFilter.Opening => x.Kind == FabricMovementKind.Opening && !x.IsReversed && !x.IsVoided,
        FabricMovementFilter.ItemIssue => x.Kind == FabricMovementKind.Issue && x.ItemId is not null,
        FabricMovementFilter.OneOffIssue => x.Kind == FabricMovementKind.Issue && x.ItemId is null,
        FabricMovementFilter.Adjustment => x.Kind == FabricMovementKind.Adjustment,
        FabricMovementFilter.Cancelled => x.IsVoided || x.IsReversed || x.Kind == FabricMovementKind.Reversal,
        _ => true
    };

    private static string[] Words(string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? []
            : search.Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Every word must be found in the fabric code or name, colour, supplier, document, order number («Ц-14» or
    /// «14»), model, destination, note or the person who recorded it.
    /// </summary>
    private static bool Matches(FabricMovement x, string[] words)
    {
        var text = string.Join('\n', new[]
        {
            x.Stock.FabricCode, FabricAliases.Canonical(x.Stock.FabricCode), x.Stock.FabricName, x.Stock.Color,
            x.SupplierName, x.Document, x.OrderNumber is { } number ? $"ц-{number} {number}" : null, x.ModelName,
            x.Destination, x.Note, x.CreatedByName
        }.Where(v => !string.IsNullOrWhiteSpace(v))).Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
        return words.All(text.Contains);
    }

    /// <summary>
    /// «Залишок пари» after each listed movement: the running sum of every movement of the merged pair (canonical
    /// fabric and colour key, as on «Залишки») in the ledger order. A voided movement adds nothing.
    /// </summary>
    public static Dictionary<int, decimal> PairBalances(IReadOnlyCollection<FabricMovement> all,
        IReadOnlyCollection<FabricMovement> rows)
    {
        static string Pair(FabricMovement x) =>
            $"{FabricAliases.Canonical(x.Stock.FabricCode)}|{StockColor.Key(x.Stock.Color)}";
        var wanted = rows.Select(Pair).ToHashSet();
        var ids = rows.Select(x => x.Id).ToHashSet();
        var result = new Dictionary<int, decimal>();
        foreach (var pair in all.Where(x => wanted.Contains(Pair(x))).GroupBy(Pair))
        {
            var balance = 0m;
            foreach (var movement in pair.OrderBy(x => x.OperationDate).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id))
            {
                if (!movement.IsVoided) balance += movement.Delta;
                if (ids.Contains(movement.Id)) result[movement.Id] = balance;
            }
        }

        return result;
    }

    /// <summary>
    /// «На 1 виріб» (TZ §6.3): the products cut for the issue each movement belongs to, by its issue line; issues made
    /// before issues by actual cutting have no count.
    /// </summary>
    private async Task<Dictionary<int, int>> ProductsAsync(IEnumerable<FabricMovement> movements,
        CancellationToken cancellationToken)
    {
        var lines = movements.Select(x => x.IssueLineId).OfType<int>().Distinct().ToArray();
        if (lines.Length == 0) return [];
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines)
            .Where(x => x.IssuedQuantity != null && x.Lines.Any(l => lines.Contains(l.Id)))
            .ToListAsync(cancellationToken);
        return issues.SelectMany(x => x.Lines.Select(l => (l.Id, Products: x.IssuedQuantity!.Value)))
            .Where(x => lines.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Products);
    }

    /// <summary>Metres of the movement per product, to thousandths; null without a products count.</summary>
    public static decimal? PerProduct(decimal delta, int? products) =>
        products is > 0 ? decimal.Round(Math.Abs(delta) / products.Value, 3, MidpointRounding.AwayFromZero) : null;

    public static FabricMovementRowResponse Map(FabricMovement x, WarehouseActor actor, int? products = null,
        decimal? pairMetersAfter = null) => new(x.Id, x.StockId,
        x.Kind.ToString(), x.OperationDate, x.CreatedAt, x.Stock.FabricCode, FabricAliases.Canonical(x.Stock.FabricCode),
        x.Stock.FabricName, x.Stock.Color, x.SupplierName, x.Document, x.OrderId, x.ItemId, x.OrderNumber, x.ModelName,
        x.Destination, x.Delta, x.MetersAfter, x.Note, x.CreatedByName, x.IsReversed, x.ReversesMovementId,
        actor.CanMoveStock && ReverseFabricMovementHandler.CanReverse(x), x.IsVoided, x.Revision,
        actor.CanMoveStock && x.CanRevise, PerProduct(x.Delta, products), pairMetersAfter);
}
