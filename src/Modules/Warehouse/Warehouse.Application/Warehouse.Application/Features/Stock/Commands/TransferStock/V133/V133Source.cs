using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Orders;
using Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133;

/// <summary>A movement of the export in one of its states (as made, or before or after a revision).</summary>
internal sealed record V133State(string StockId, DateOnly OperationDate, long Delta, string? Supplier,
    string? Reference, bool IsVoided);

/// <summary>An edit or void of a movement in the export.</summary>
/// <param name="BeforeKind">The kind before the revision: a voided movement keeps it only here.</param>
internal sealed record V133Revision(string Id, Guid OperationId, DateTimeOffset At, string? ActorEmail,
    V133State Before, V133State After, string? BeforeKind);

/// <summary>A movement of the export: its kind and state as first made, its revisions in order, and its row now.</summary>
internal sealed record V133Movement(V133MovementRow Row, FabricMovementKind Kind, DateTimeOffset At,
    V133State Original, IReadOnlyList<V133Revision> Revisions);

/// <summary>
/// Reads the D1 rows of the old «Цех» and checks them before anything is written (TZ §19, §19.1, §20): unique ids, every
/// link has its parent, before + change = after, the chain of every line by operation date, record time and id, the
/// balance of every line, and the issued metres of every package line. A value the export cut short
/// («[truncated for model]») is refused wherever the transfer reads it.
/// </summary>
internal sealed partial class V133Source
{
    public const string TruncatedMarker = "[truncated for model]";

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    private readonly List<StockTransferProblem> _errors = [];
    private readonly List<StockTransferProblem> _notes = [];

    private V133Source(StockTransferInput input) => Input = input;

    public StockTransferInput Input { get; }
    public IReadOnlyList<StockTransferProblem> Errors => _errors;
    public IReadOnlyList<StockTransferProblem> Notes => _notes;

    public Dictionary<string, V133StockRow> Stocks { get; } = new(StringComparer.Ordinal);

    /// <summary>In the order of the chain: operation date, record time, id.</summary>
    public List<V133Movement> Movements { get; } = [];

    public Dictionary<string, V133Movement> MovementsById { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, V133BatchRow> Batches { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, V133IssueLineRow> Lines { get; } = new(StringComparer.Ordinal);

    /// <summary>The movements of each package line.</summary>
    public Dictionary<string, List<string>> LineMovements { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, V133OrderRow> Orders { get; } = new(StringComparer.Ordinal);

    /// <summary>Old item id → its order and position (from 1, by the order of the rows in the file).</summary>
    public Dictionary<string, (V133ItemRow Item, int Position)> Items { get; } = new(StringComparer.Ordinal);

    public static V133Source Read(StockTransferInput input)
    {
        var source = new V133Source(input);
        source.ReadRows();
        var linked = source._errors.Count == 0;
        source.CheckLedger();
        if (linked) source.CheckIssues();
        return source;
    }

    public void Error(string table, string? rowId, string message) => _errors.Add(new(table, rowId, message));

    public void Note(string table, string? rowId, string message) => _notes.Add(new(table, rowId, message));

    public StockTransferSource Summary()
    {
        var pairs = Stocks.Values.GroupBy(x => (FabricAliases.Canonical(x.FabricId), StockColor.Key(x.Color)))
            .Select(g => g.Sum(x => x.HundredthsOfMeter)).ToList();
        int Count(string kind) => Input.FabricStockMovements.Count(x => x.Kind == kind);
        return new StockTransferSource(Input.FabricStock.Count, pairs.Count, Input.FabricStockMovements.Count,
            Count("opening"), Count("receipt"), Count("issue"), Count("adjustment"), Count("void"),
            Input.FabricMovementRevisions.Count, Input.FabricIssueBatches.Count, Input.FabricIssueLines.Count,
            Input.FabricReservations.Count, Input.FabricStock.Sum(x => x.HundredthsOfMeter) / 100m,
            pairs.Count(x => x < 0), pairs.Count(x => x == 0));
    }

    /// <summary>Products cut for a package: its result says so for an issue by actual cutting; null before those.</summary>
    public int? IssuedQuantity(V133BatchRow batch)
    {
        var text = batch.ResultJson ?? string.Empty;
        var cut = text.IndexOf(TruncatedMarker, StringComparison.Ordinal);
        var head = cut < 0 ? text : text[..cut];
        var match = IssuedQuantityPattern().Match(head);
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture,
            out var value) && value > 0
            ? value
            : null;
    }

    public static DateOnly KyivDate(DateTimeOffset at) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(at, Kyiv).Date);

    private void ReadRows()
    {
        Truncated();
        Unique("production_orders", Input.ProductionOrders, x => x.Id);
        foreach (var order in Input.ProductionOrders) Orders.TryAdd(order.Id, order);

        Unique("production_items", Input.ProductionItems, x => x.Id);
        foreach (var group in Input.ProductionItems.GroupBy(x => x.OrderId, StringComparer.Ordinal))
        {
            if (!Orders.ContainsKey(group.Key))
                foreach (var item in group) Error("production_items", item.Id, "Немає завдання цієї позиції.");
            var position = 0;
            foreach (var item in group) Items.TryAdd(item.Id, (item, ++position));
        }

        Unique("fabric_stock", Input.FabricStock, x => x.Id);
        foreach (var stock in Input.FabricStock)
        {
            if (!Stocks.TryAdd(stock.Id, stock)) continue;
            if (string.IsNullOrWhiteSpace(stock.FabricId) || string.IsNullOrWhiteSpace(stock.Color))
                Error("fabric_stock", stock.Id, "Немає коду тканини або кольору.");
            else if (!FabricStock.IsValidMeters(stock.HundredthsOfMeter / 100m))
                Error("fabric_stock", stock.Id, "Забагато метрів.");
        }

        foreach (var pair in Input.FabricStock.Where(x => !string.IsNullOrWhiteSpace(x.FabricId))
                     .GroupBy(x => (Code: FabricAliases.Normalize(x.FabricId), Key: StockColor.Key(x.Color ?? "")))
                     .Where(g => g.Count() > 1))
            Error("fabric_stock", pair.First().Id,
                $"Два рядки {pair.Key.Code} одного кольору «{pair.First().Color}»: у Brevi рядок тканини і кольору один.");

        var revisions = Revisions();
        Unique("fabric_stock_movements", Input.FabricStockMovements, x => x.Id);
        foreach (var row in Input.FabricStockMovements)
        {
            if (MovementsById.ContainsKey(row.Id)) continue;
            var movement = Movement(row, revisions.GetValueOrDefault(row.Id) ?? []);
            if (movement is null) continue;
            MovementsById[row.Id] = movement;
            Movements.Add(movement);
        }

        foreach (var id in revisions.Keys.Where(x => !MovementsById.ContainsKey(x)))
            Error("fabric_movement_revisions", id, "Правка руху, якого немає.");
        Movements.Sort((a, b) => Chain(a.Row, b.Row));

        foreach (var stock in Stocks.Values.Where(x => x.LastMovementId is { Length: > 0 } id &&
                                                       (!MovementsById.TryGetValue(id, out var m) ||
                                                        m.Row.StockId != x.Id)))
            Error("fabric_stock", stock.Id, "Останній рух рядка не належить йому.");

        Unique("fabric_issue_batches", Input.FabricIssueBatches, x => x.Id);
        foreach (var batch in Input.FabricIssueBatches)
        {
            if (!Batches.TryAdd(batch.Id, batch)) continue;
            if (!Items.ContainsKey(batch.ItemId)) Error("fabric_issue_batches", batch.Id, "Немає позиції завдання.");
            if (batch.ResultJson?.Contains(TruncatedMarker, StringComparison.Ordinal) == true)
                Note("fabric_issue_batches", batch.Id,
                    "Результат пакета обрізано у вигрузці: перенесено лише кількість виробів з його початку.");
        }

        Unique("fabric_issue_lines", Input.FabricIssueLines, x => x.Id);
        foreach (var line in Input.FabricIssueLines)
        {
            if (!Lines.TryAdd(line.Id, line)) continue;
            if (!Batches.TryGetValue(line.BatchId, out var batch))
                Error("fabric_issue_lines", line.Id, "Немає пакета видачі.");
            else if (batch.ItemId != line.ItemId) Error("fabric_issue_lines", line.Id, "Позиція рядка інша, ніж у пакета.");
            if (!Stocks.ContainsKey(line.StockId)) Error("fabric_issue_lines", line.Id, "Немає рядка складу.");
            if (line.Purpose is not ("planned" or "replacement" or "additional"))
                Error("fabric_issue_lines", line.Id, $"Невідоме призначення «{line.Purpose}».");
        }

        var linked = new HashSet<string>(StringComparer.Ordinal);
        foreach (var component in Input.FabricIssueComponents)
        {
            if (!linked.Add(component.MovementId))
                Error("fabric_issue_components", component.MovementId, "Рух належить двом рядкам видачі.");
            else if (!MovementsById.ContainsKey(component.MovementId))
                Error("fabric_issue_components", component.MovementId, "Немає руху.");
            else if (!Lines.ContainsKey(component.LineId))
                Error("fabric_issue_components", component.LineId, "Немає рядка видачі.");
            else
            {
                if (!LineMovements.TryGetValue(component.LineId, out var list))
                    LineMovements[component.LineId] = list = [];
                list.Add(component.MovementId);
            }
        }

        foreach (var line in Lines.Values.Where(x => !LineMovements.ContainsKey(x.Id)))
            Error("fabric_issue_lines", line.Id, "Рядок видачі без руху (fabric_issue_components).");

        foreach (var reservation in Input.FabricReservations.Where(x => !Items.ContainsKey(x.ItemId)))
            Error("fabric_reservations", reservation.Id, "Немає позиції завдання.");
    }

    private void CheckLedger()
    {
        foreach (var movement in Movements)
        {
            var row = movement.Row;
            if (row.BeforeHundredths + row.DeltaHundredths != row.AfterHundredths)
                Error("fabric_stock_movements", row.Id,
                    $"Було {Meters(row.BeforeHundredths)} + зміна {Meters(row.DeltaHundredths)} ≠ стало {Meters(row.AfterHundredths)}.");
            if (row.Kind == "void" && row.DeltaHundredths != 0)
                Error("fabric_stock_movements", row.Id, "Скасований рух має зміну, а не нуль.");
        }

        foreach (var (stockId, chain) in Movements.GroupBy(x => x.Row.StockId).Select(g => (g.Key, g.ToList())))
        {
            var balance = 0L;
            foreach (var movement in chain)
            {
                if (movement.Row.BeforeHundredths != balance)
                {
                    Error("fabric_stock_movements", movement.Row.Id,
                        $"Ланцюжок рядка розірвано: перед рухом {Meters(balance)} м, а в русі «було» {Meters(movement.Row.BeforeHundredths)} м.");
                    break;
                }

                balance = movement.Row.AfterHundredths;
            }

            if (Stocks.TryGetValue(stockId, out var stock) && balance != stock.HundredthsOfMeter)
                Error("fabric_stock", stockId,
                    $"Рухи дають {Meters(balance)} м, а залишок рядка {Meters(stock.HundredthsOfMeter)} м.");
        }

        foreach (var stock in Stocks.Values.Where(x => x.HundredthsOfMeter != 0 &&
                                                       Movements.All(m => m.Row.StockId != x.Id)))
            Error("fabric_stock", stock.Id, $"Залишок {Meters(stock.HundredthsOfMeter)} м без жодного руху.");
    }

    private void CheckIssues()
    {
        foreach (var line in Lines.Values)
        {
            var movements = LineMovements[line.Id].Select(x => MovementsById[x]).ToList();
            var voided = movements.All(x => x.Row.Kind == "void");
            if (line.VoidedAt is not null != voided)
            {
                Error("fabric_issue_lines", line.Id, line.VoidedAt is null
                    ? "Рядок видачі діє, а всі його рухи скасовано."
                    : "Рядок видачі скасовано, а його рухи діють.");
                continue;
            }

            if (movements.Any(x => x.Kind != FabricMovementKind.Issue))
                Error("fabric_issue_lines", line.Id, "Рядок видачі посилається не на видачу.");
            if (movements.Any(x => x.Row.ItemId is { } itemId && itemId != line.ItemId))
                Error("fabric_issue_lines", line.Id, "Рух рядка видачі іншої позиції.");
            if (!voided && -movements.Where(x => x.Row.Kind != "void").Sum(x => x.Row.DeltaHundredths) !=
                line.ActualHundredths)
                Error("fabric_issue_lines", line.Id,
                    $"Рухи рядка дають {Meters(-movements.Sum(x => x.Row.DeltaHundredths))} м, а видано {Meters(line.ActualHundredths)} м.");
        }
    }

    private Dictionary<string, List<V133Revision>> Revisions()
    {
        var result = new Dictionary<string, List<V133Revision>>(StringComparer.Ordinal);
        Unique("fabric_movement_revisions", Input.FabricMovementRevisions, x => x.Id);
        foreach (var row in Input.FabricMovementRevisions.DistinctBy(x => x.Id))
        {
            var before = State(row.BeforeJson, out var beforeKind);
            var after = State(row.AfterJson, out _);
            if (before is null || after is null || !Time(row.At, out var at))
            {
                Error("fabric_movement_revisions", row.Id, "Правку не прочитано: стан до чи після неповний.");
                continue;
            }

            if (!result.TryGetValue(row.MovementId, out var list)) result[row.MovementId] = list = [];
            list.Add(new V133Revision(row.Id, Guid.TryParse(row.OperationId, out var op) ? op : Guid.NewGuid(), at,
                row.ActorEmail, before, after, beforeKind));
        }

        foreach (var list in result.Values) list.Sort((a, b) => a.At.CompareTo(b.At));
        return result;
    }

    private V133Movement? Movement(V133MovementRow row, List<V133Revision> revisions)
    {
        if (!Stocks.ContainsKey(row.StockId))
        {
            Error("fabric_stock_movements", row.Id, "Немає рядка складу.");
            return null;
        }

        if (!Time(row.At, out var at))
        {
            Error("fabric_stock_movements", row.Id, "Невідомий час запису.");
            return null;
        }

        var date = Date(row.OperationDate) ?? KyivDate(at);
        var now = new V133State(row.StockId, date, row.DeltaHundredths, Text(row.Supplier), Text(row.Reference),
            row.Kind == "void");
        var kindText = row.Kind;
        if (revisions.Count > 0)
        {
            kindText = revisions[0].BeforeKind ?? row.Kind;
            var last = revisions[^1].After;
            if (last != now)
                Error("fabric_stock_movements", row.Id, "Остання правка руху не збігається з рухом.");
            foreach (var revision in revisions.Where(x => !Stocks.ContainsKey(x.Before.StockId) ||
                                                          !Stocks.ContainsKey(x.After.StockId)))
                Error("fabric_movement_revisions", revision.Id, "Правка посилається на рядок складу, якого немає.");
        }

        if (Kind(kindText) is not { } kind)
        {
            Error("fabric_stock_movements", row.Id, kindText == "void"
                ? "Скасований рух без правки: невідомо, яким він був."
                : $"Невідомий вид руху «{kindText}».");
            return null;
        }

        if (row.ItemId is { Length: > 0 } item && !Items.ContainsKey(item))
            Error("fabric_stock_movements", row.Id, "Немає позиції завдання руху.");
        return new V133Movement(row, kind, at, revisions.Count > 0 ? revisions[0].Before : now, revisions);
    }

    /// <summary>The movement of a revision's JSON: {"movement": {stockId, operationDate, kind, deltaHundredths, …}}.</summary>
    private static V133State? State(string json, out string? kind)
    {
        kind = null;
        if (json.Contains(TruncatedMarker, StringComparison.Ordinal)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("movement", out var m) || m.ValueKind != JsonValueKind.Object)
                return null;
            kind = String(m, "kind");
            var stockId = String(m, "stockId");
            var date = Date(String(m, "operationDate"));
            if (stockId is null || date is null || !m.TryGetProperty("deltaHundredths", out var delta) ||
                delta.ValueKind != JsonValueKind.Number || !delta.TryGetInt64(out var value)) return null;
            return new V133State(stockId, date.Value, value, Text(String(m, "supplier")), Text(String(m, "reference")),
                kind == "void");
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? String(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static FabricMovementKind? Kind(string kind) => kind switch
    {
        "opening" => FabricMovementKind.Opening,
        "receipt" => FabricMovementKind.Receipt,
        "issue" => FabricMovementKind.Issue,
        "adjustment" => FabricMovementKind.Adjustment,
        _ => null
    };

    private void Truncated()
    {
        void Check(string table, string id, params string?[] values)
        {
            if (values.Any(x => x?.Contains(TruncatedMarker, StringComparison.Ordinal) == true))
                Error(table, id, "Значення обрізано у вигрузці («[truncated for model]»): потрібна повна вигрузка.");
        }

        foreach (var x in Input.FabricStock) Check("fabric_stock", x.Id, x.FabricId, x.FabricName, x.Color);
        foreach (var x in Input.FabricStockMovements)
            Check("fabric_stock_movements", x.Id, x.Supplier, x.Reference, x.Note, x.ModelName, x.ClientName);
        foreach (var x in Input.FabricMovementRevisions)
            Check("fabric_movement_revisions", x.Id, x.BeforeJson, x.AfterJson);
        foreach (var x in Input.FabricIssueLines)
            Check("fabric_issue_lines", x.Id, x.PlannedComponentJson, x.Note, x.PlanKey);
        foreach (var x in Input.ProductionItems) Check("production_items", x.Id, x.ModelName, x.ModelArticle);
    }

    private void Unique<T>(string table, IEnumerable<T> rows, Func<T, string> id)
    {
        foreach (var duplicate in rows.GroupBy(id, StringComparer.Ordinal).Where(g => g.Count() > 1))
            Error(table, duplicate.Key, "Повторюється id рядка.");
    }

    /// <summary>The order of a chain in the old «Цех»: operation date, record time, id.</summary>
    private int Chain(V133MovementRow a, V133MovementRow b)
    {
        var x = MovementsById[a.Id];
        var y = MovementsById[b.Id];
        var byDate = (Date(a.OperationDate) ?? KyivDate(x.At)).CompareTo(Date(b.OperationDate) ?? KyivDate(y.At));
        if (byDate != 0) return byDate;
        var time = x.At.CompareTo(y.At);
        return time != 0 ? time : string.CompareOrdinal(a.Id, b.Id);
    }

    public static bool Time(string? text, out DateTimeOffset at) =>
        DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out at);

    public static DateOnly? Date(string? text) =>
        DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string Meters(long hundredths) => (hundredths / 100m).ToString("0.00", CultureInfo.InvariantCulture);

    [GeneratedRegex("\"issuedQuantity\"\\s*:\\s*(\\d+)")]
    private static partial Regex IssuedQuantityPattern();
}
