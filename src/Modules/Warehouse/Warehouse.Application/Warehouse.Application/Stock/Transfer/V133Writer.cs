using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Transfer;

/// <summary>
/// Writes a checked export into an empty stock (TZ §19): the lines, then every movement as it was first made, then its
/// revisions in time order through the domain (edit, void, restore), the chains restated, and only then a comparison of
/// every movement and line with the export. Packages of issues, their lines over the movements, and the saved plan of
/// each item issued come last. Any difference is an error and the caller rolls the whole transfer back.
/// </summary>
internal sealed class V133Writer(
    V133Source source,
    IReadOnlyDictionary<string, V133Target?> targets,
    IReadOnlyDictionary<string, WarehouseFabric> fabrics,
    Func<string?, CancellationToken, Task<string>> nameOf,
    DateTimeOffset now)
{
    private readonly Dictionary<string, FabricStock> _lines = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FabricMovement> _movements = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FabricMovementRevision> _revisions = new(StringComparer.Ordinal);

    public async Task<StockTransferTarget?> WriteAsync(IWarehouseDbContext db, CancellationToken cancellationToken)
    {
        var missing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in source.Stocks.Values)
        {
            var code = FabricAliases.Normalize(row.FabricId);
            var fabric = fabrics.GetValueOrDefault(code);
            if (fabric is null && missing.Add(code))
                source.Note("fabric_stock", row.Id,
                    $"Тканини {code} немає в «Тканини»: її рядки перенесено з назвою старого «Цеху», без ціни.");
            var first = source.Movements.Where(x => x.Original.StockId == row.Id).Select(x => (DateTimeOffset?)x.At)
                .Min();
            var line = new FabricStock(new StockFabric(fabric?.Id, code, fabric?.Name ?? row.FabricName ?? code),
                row.Color, first ?? now, row.Id);
            _lines[row.Id] = line;
            db.FabricStocks.Add(line);
        }

        foreach (var movement in source.Movements.OrderBy(x => x.At).ThenBy(x => x.Row.Id, StringComparer.Ordinal))
        {
            var row = movement.Row;
            var original = movement.Original;
            if (original.IsVoided)
            {
                source.Error("fabric_stock_movements", row.Id, "Рух скасовано вже в першому стані: не переноситься.");
                continue;
            }

            var target = row.ItemId is { Length: > 0 } item ? targets.GetValueOrDefault(item) : null;
            var destination = movement.Kind != FabricMovementKind.Issue ? null
                : target is null && row.ItemId is { Length: > 0 } ? $"Видалене завдання {row.OrderNumber}".Trim()
                : target is null ? V133Source.Text(row.ClientName) : null;
            var details = new FabricMovementDetails(original.OperationDate,
                Cut(V133Source.Text(row.Note), FabricMovement.NoteMaxLength), null,
                Cut(original.Supplier, FabricMovement.SupplierMaxLength),
                Cut(original.Reference, FabricMovement.DocumentMaxLength), target?.Target,
                Cut(destination, FabricMovement.FormTextMaxLength));
            _movements[row.Id] = _lines[original.StockId].Import(movement.Kind, original.Delta / 100m, details,
                await nameOf(row.ActorEmail, cancellationToken), movement.At, row.Id);
        }

        if (source.Errors.Count > 0) return null;
        await db.SaveChangesAsync(cancellationToken);

        foreach (var (movement, revision) in source.Movements
                     .SelectMany(m => m.Revisions.Select(r => (Movement: m, Revision: r)))
                     .OrderBy(x => x.Revision.At).ThenBy(x => x.Revision.Id, StringComparer.Ordinal))
            await ReviseAsync(db, movement, revision, cancellationToken);
        if (source.Errors.Count > 0) return null;

        var all = _movements.Values.ToList();
        foreach (var line in _lines.Values) line.Restate(all);
        Compare();
        if (source.Errors.Count > 0) return null;

        var (issues, issueLines, plans) = await IssuesAsync(db, cancellationToken);
        if (source.Errors.Count > 0) return null;
        await db.SaveChangesAsync(cancellationToken);
        return new StockTransferTarget(_lines.Count, _movements.Count, _revisions.Count, issues, issueLines, plans,
            _lines.Values.Sum(x => x.Meters));
    }

    private async Task ReviseAsync(IWarehouseDbContext db, V133Movement movement, V133Revision revision,
        CancellationToken cancellationToken)
    {
        var saved = _movements[movement.Row.Id];
        var before = revision.Before;
        if (saved.Stock.LegacyId != before.StockId || saved.OperationDate != before.OperationDate ||
            saved.Delta != before.Delta / 100m || saved.IsVoided != before.IsVoided)
        {
            source.Error("fabric_movement_revisions", revision.Id, "Стан руху «до правки» не збігається з попереднім.");
            return;
        }

        var after = revision.After;
        var line = _lines[after.StockId];
        var previous = movement.Revisions.TakeWhile(x => x != revision).LastOrDefault();
        var restores = previous is not null && previous.Before == after && _revisions.TryGetValue(previous.Id,
            out var undone) && !undone.IsUndone
            ? undone
            : null;
        var action = restores is not null ? FabricMovementRevisionAction.Restore
            : after.IsVoided && !before.IsVoided ? FabricMovementRevisionAction.Void
            : FabricMovementRevisionAction.Edit;
        var state = new FabricMovementState(line.Id, after.OperationDate, after.Delta / 100m,
            Cut(after.Supplier, FabricMovement.SupplierMaxLength), Cut(after.Reference, FabricMovement.DocumentMaxLength),
            saved.Destination, after.IsVoided);
        try
        {
            var change = line.Revise(saved, state, action, null,
                await nameOf(revision.ActorEmail, cancellationToken), revision.At, revision.OperationId, restores);
            _revisions[revision.Id] = change;
            db.FabricMovementRevisions.Add(change);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            source.Error("fabric_movement_revisions", revision.Id, "Правку не можна застосувати за правилами складу.");
        }
    }

    /// <summary>TZ §19.1: every movement and line after the transfer is the same as in the export.</summary>
    private void Compare()
    {
        foreach (var movement in source.Movements)
        {
            var row = movement.Row;
            var saved = _movements[row.Id];
            var date = V133Source.Date(row.OperationDate) ?? V133Source.KyivDate(movement.At);
            if (saved.Stock.LegacyId != row.StockId || saved.OperationDate != date ||
                saved.Delta * 100m != row.DeltaHundredths || saved.MetersBefore * 100m != row.BeforeHundredths ||
                saved.MetersAfter * 100m != row.AfterHundredths || saved.IsVoided != (row.Kind == "void"))
                source.Error("fabric_stock_movements", row.Id,
                    $"Після перенесення рух інший: було {saved.MetersBefore}, зміна {saved.Delta}, стало {saved.MetersAfter}.");
        }

        foreach (var (id, line) in _lines)
            if (line.Meters * 100m != source.Stocks[id].HundredthsOfMeter)
                source.Error("fabric_stock", id, $"Після перенесення залишок {line.Meters} м, а у вигрузці " +
                                                 $"{V133Source.Meters(source.Stocks[id].HundredthsOfMeter)} м.");
    }

    private async Task<(int Issues, int Lines, int Plans)> IssuesAsync(IWarehouseDbContext db,
        CancellationToken cancellationToken)
    {
        var lines = source.Lines.Values.ToLookup(x => x.BatchId);
        var planned = new HashSet<int>();
        var (issueCount, lineCount, planCount) = (0, 0, 0);
        foreach (var batch in source.Batches.Values.OrderBy(x => x.CreatedAt, StringComparer.Ordinal)
                     .ThenBy(x => x.Id, StringComparer.Ordinal))
        {
            if (targets.GetValueOrDefault(batch.ItemId) is not { } target) continue;
            var rows = lines[batch.Id].OrderBy(x => x.CreatedAt, StringComparer.Ordinal)
                .ThenBy(x => x.Id, StringComparer.Ordinal).ToList();
            if (rows.Count == 0)
            {
                source.Note("fabric_issue_batches", batch.Id, "Пакет без рядків: не переноситься.");
                continue;
            }

            if (rows.Count > FabricIssue.MaxLines)
            {
                source.Error("fabric_issue_batches", batch.Id, $"У пакеті понад {FabricIssue.MaxLines} рядків.");
                continue;
            }

            var movements = rows.ToDictionary(x => x.Id,
                x => source.LineMovements[x.Id].Select(id => _movements[id]).ToList());
            int? version = null;
            if (planned.Add(target.Target.ItemId) && Plan(batch.ItemId, target) is { } plan)
            {
                db.FabricItemPlans.Add(plan);
                planCount++;
                version = 1;
            }
            else if (db.FabricItemPlans.Local.Any(x => x.ItemId == target.Target.ItemId)) version = 1;

            var quantity = source.IssuedQuantity(batch);
            var counted = source.LineMovements[rows[0].Id].Select(x => source.MovementsById[x].Row.ItemQuantity)
                .FirstOrDefault();
            if (quantity is not null && counted is not null && counted != quantity)
                source.Note("fabric_issue_batches", batch.Id,
                    $"Кількість виробів пакета {quantity}, а в його русі {counted}: взято {quantity}.");
            V133Source.Time(batch.CreatedAt, out var at);
            var issuedOn = movements.Values.SelectMany(x => x).Select(x => x.OperationDate).Min();
            var issue = new FabricIssue(target.Target, issuedOn, null, await nameOf(batch.ActorEmail, cancellationToken),
                at, batch.Id, quantity, null, version);
            foreach (var row in rows)
            {
                var purpose = row.Purpose switch
                {
                    "planned" => FabricIssuePurpose.Planned,
                    "replacement" => FabricIssuePurpose.Replacement,
                    _ => FabricIssuePurpose.Additional
                };
                var key = purpose == FabricIssuePurpose.Additional ? null : PlanKey(row);
                if (purpose != FabricIssuePurpose.Additional && key is null)
                {
                    // The plan named a fabric without a «Тканини» code; it is never bound by a similar name (TZ §19).
                    source.Note("fabric_issue_lines", row.Id,
                        $"План рядка без коду тканини («{row.PlannedColor}», {V133Source.Meters(row.PlannedHundredths ?? 0)} м): перенесено як додатковий, план не покриває.");
                    purpose = FabricIssuePurpose.Additional;
                }

                var covered = purpose == FabricIssuePurpose.Additional ? 0m : row.CoveredPlanHundredths / 100m;
                if (row.Purpose == "additional" && row.CoveredPlanHundredths != 0)
                    source.Note("fabric_issue_lines", row.Id, "Додатковий рядок мав покриття плану: не перенесено.");
                issue.AddLine(purpose, key, row.PlannedHundredths / 100m, movements[row.Id], covered,
                    Component(row.PlannedComponentJson).Layer, Cut(V133Source.Text(row.Note), FabricIssueLine.NoteMaxLength));
                lineCount++;
            }

            db.FabricIssues.Add(issue);
            issueCount++;
        }

        return (issueCount, lineCount, planCount);
    }

    /// <summary>
    /// The saved plan of an item issued in the old «Цех»: its saved main-fabric lines (<c>fabric_reservations</c>) and
    /// the planned components its issue lines kept, for its whole quantity.
    /// </summary>
    private FabricItemPlan? Plan(string itemId, V133Target target)
    {
        var parts = new List<FabricItemPlanPart>();

        void Add(string? code, string? color, FabricLayer layer, long? hundredths)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(color) || hundredths is not > 0) return;
            var normalized = FabricAliases.Normalize(code);
            var key = StockColor.Key(color);
            if (parts.Any(x => x.FabricCode == normalized && x.ColorKey == key && x.Layer == layer)) return;
            parts.Add(new FabricItemPlanPart(normalized, StockColor.Clean(color), key, layer, hundredths.Value / 100m));
        }

        foreach (var row in source.Input.FabricReservations.Where(x => x.ItemId == itemId))
            Add(row.FabricId, row.Color, FabricLayer.Upper, row.HundredthsOfMeter);
        foreach (var component in source.Lines.Values.Where(x => x.ItemId == itemId)
                     .Select(x => Component(x.PlannedComponentJson)).Where(x => x.Code is not null))
            Add(component.Code, component.Color, component.Layer ?? FabricLayer.Upper, component.Hundredths);
        if (parts.Count == 0) return null;
        if (target.Quantity <= 0)
        {
            source.Note("production_items", itemId, "У позиції Brevi немає кількості: план не збережено.");
            return null;
        }

        var first = source.Batches.Values.Where(x => x.ItemId == itemId).Min(x => x.CreatedAt);
        V133Source.Time(first, out var at);
        return new FabricItemPlan(target.Target.OrderId, target.Target.ItemId, target.Quantity, parts,
            "Перенесення зі старого «Цеху»", at);
    }

    /// <summary>The planned part of a line: its planned fabric and colour, or the key <c>["FAB-…","колір"]</c>.</summary>
    private static FabricPlanKey? PlanKey(V133IssueLineRow row)
    {
        var code = V133Source.Text(row.PlannedFabricId);
        var color = V133Source.Text(row.PlannedColor);
        if ((code is null || color is null) && row.PlanKey is { } text)
        {
            try
            {
                var parts = JsonSerializer.Deserialize<string[]>(text);
                if (parts is { Length: 2 } && parts[0] != "component")
                {
                    code ??= parts[0];
                    color ??= parts[1];
                }
            }
            catch (JsonException)
            {
            }
        }

        return code is null || color is null ? null : new FabricPlanKey(FabricAliases.Normalize(code), StockColor.Key(color));
    }

    /// <summary>
    /// <c>planned_component_json</c>: {"component": {fabricId, color, role, plannedHundredths}, "actualRole"}. The layer is
    /// the component's role, or the role an additional line was given.
    /// </summary>
    private static (string? Code, string? Color, FabricLayer? Layer, long? Hundredths) Component(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var role = Role(root, "actualRole");
            if (!root.TryGetProperty("component", out var c) || c.ValueKind != JsonValueKind.Object)
                return (null, null, role, null);
            long? hundredths = c.TryGetProperty("plannedHundredths", out var h) && h.ValueKind == JsonValueKind.Number && h.TryGetInt64(out var v) ? v : null;
            return (Text(c, "fabricId"), Text(c, "color"), Role(c, "role") ?? role, hundredths);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static FabricLayer? Role(JsonElement element, string name) => Text(element, name) switch
    {
        "upper" => FabricLayer.Upper,
        "lining" => FabricLayer.Lining,
        "insulation" => FabricLayer.Insulation,
        "fleece" => FabricLayer.Fleece,
        _ => null
    };

    private static string? Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? V133Source.Text(value.GetString())
            : null;

    private static string? Cut(string? value, int max) => value is null || value.Length <= max ? value : value[..max];
}
