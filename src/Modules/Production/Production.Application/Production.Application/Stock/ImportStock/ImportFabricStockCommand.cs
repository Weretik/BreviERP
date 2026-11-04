using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;
using Production.Domain.Stock;

namespace Production.Application.Stock.ImportStock;

/// <summary>
/// Moves «Склад тканин» of the old «Цех» (PRD-D32): lines with their movements in order, and the issue packages of
/// orders already moved by «Імпорт». <paramref name="DryRun"/> only checks. Nothing is saved unless every row is fine;
/// a line already moved (by its old id) is skipped, so a repeat changes nothing.
/// </summary>
public sealed record ImportFabricStockCommand(ProductionActor Actor, FabricStockImportInput Input, bool DryRun)
    : ICommand<Result<FabricStockImportResult>>;

public sealed class ImportFabricStockCommandValidator : AbstractValidator<ImportFabricStockCommand>
{
    public ImportFabricStockCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Stocks).NotNull().Must(x => x.Count is > 0 and <= 5000)
                .WithMessage("У файлі має бути від 1 до 5000 рядків складу.");
            RuleFor(x => x.Input.Issues).NotNull().Must(x => x.Count <= 5000);
            RuleFor(x => x.Input.OldReservations).NotNull();
            RuleForEach(x => x.Input.Stocks).ChildRules(stock =>
            {
                stock.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(FabricStock.LegacyIdMaxLength);
                stock.RuleFor(x => x.FabricCode).NotEmpty().MaximumLength(FabricStock.CodeMaxLength);
                stock.RuleFor(x => x.Color).NotEmpty().MaximumLength(StockColor.MaxLength);
                stock.RuleFor(x => x.Movements).NotNull().Must(x => x.Count is > 0 and <= 10000);
                stock.RuleForEach(x => x.Movements).ChildRules(m =>
                {
                    m.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(FabricMovement.LegacyIdMaxLength);
                    m.RuleFor(x => x.Kind).Must(x => Enum.TryParse<FabricMovementKind>(x, out var k) && Enum.IsDefined(k))
                        .WithMessage("Невідомий вид руху.");
                    m.RuleFor(x => x.Supplier).MaximumLength(FabricMovement.SupplierMaxLength);
                    m.RuleFor(x => x.Document).MaximumLength(FabricMovement.DocumentMaxLength);
                    m.RuleFor(x => x.Note).MaximumLength(FabricMovement.NoteMaxLength);
                    m.RuleFor(x => x.OrderLegacyId).MaximumLength(ProductionOrder.LegacyIdMaxLength);
                }).When(x => x.Movements is not null);
            }).When(x => x.Input.Stocks is not null);
            RuleForEach(x => x.Input.Issues).ChildRules(issue =>
            {
                issue.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(FabricStock.LegacyIdMaxLength);
                issue.RuleFor(x => x.OrderLegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
                issue.RuleFor(x => x.Position).GreaterThan(0);
                issue.RuleFor(x => x.Note).MaximumLength(FabricIssue.NoteMaxLength);
                issue.RuleFor(x => x.Lines).NotNull().Must(x => x.Count is > 0 and <= FabricIssue.MaxLines);
                issue.RuleForEach(x => x.Lines).ChildRules(l =>
                {
                    l.RuleFor(x => x.Purpose).Must(x => Enum.TryParse<FabricIssuePurpose>(x, out var p) && Enum.IsDefined(p))
                        .WithMessage("Невідоме призначення видачі.");
                    l.RuleFor(x => x.MovementLegacyId).NotEmpty();
                    l.RuleFor(x => x.CoveredMeters).GreaterThanOrEqualTo(0);
                }).When(x => x.Lines is not null);
            }).When(x => x.Input.Issues is not null);
        });
    }
}

public sealed class ImportFabricStockCommandHandler(IProductionDbContext db, IProductionFabricReader fabrics,
    IProductionStaffReader staff, StockReservations reservations)
    : ICommandHandler<ImportFabricStockCommand, Result<FabricStockImportResult>>
{
    private const string Transfer = "Перенесення зі старого «Цеху»";

    public async ValueTask<Result<FabricStockImportResult>> Handle(ImportFabricStockCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete) return Result<FabricStockImportResult>.Forbidden(StockMessages.CannotImport);
        var input = command.Input;

        var byCode = (await fabrics.ListAsync(cancellationToken)).Where(x => x.Code != null)
            .GroupBy(x => x.Code!.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First());
        var moved = (await db.FabricStocks.AsNoTracking().Where(x => x.LegacyId != null)
            .Select(x => x.LegacyId!).ToListAsync(cancellationToken)).ToHashSet();
        var keys = (await db.FabricStocks.AsNoTracking().Select(x => new { x.FabricCode, x.ColorKey, x.LegacyId })
            .ToListAsync(cancellationToken)).ToDictionary(x => (x.FabricCode, x.ColorKey), x => x.LegacyId);
        var movedIssues = (await db.FabricIssues.AsNoTracking().Where(x => x.LegacyId != null)
            .Select(x => x.LegacyId!).ToListAsync(cancellationToken)).ToHashSet();
        var orderIds = input.Stocks.SelectMany(x => x.Movements).Select(x => x.OrderLegacyId)
            .Concat(input.Issues.Select(x => x.OrderLegacyId)).OfType<string>().Select(x => x.Trim()).Distinct().ToArray();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.LegacyId != null && orderIds.Contains(x.LegacyId) && x.DeletedAt == null)
            .ToDictionaryAsync(x => x.LegacyId!, cancellationToken);
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        FabricMovementTarget? TargetOf(string? orderLegacyId, int? position) =>
            orderLegacyId is not null && position is { } p && orders.TryGetValue(orderLegacyId.Trim(), out var order) &&
            order.Items.FirstOrDefault(x => x.Position == p) is { } item
                ? new FabricMovementTarget(order.Id, item.Id, order.Number, item.ModelName)
                : null;

        var lines = new List<FabricStockImportLine>();
        var plans = new List<(FabricStockImportRow Row, ProductionFabric Fabric)>();
        var fileMovements = new Dictionary<string, FabricMovementImportRow>();
        foreach (var row in input.Stocks)
        {
            var code = row.FabricCode.Trim().ToUpperInvariant();
            var colorKey = StockColor.Key(row.Color);
            string? error = null;
            if (moved.Contains(row.LegacyId.Trim()))
            {
                lines.Add(new FabricStockImportLine(code, row.Color, row.Movements.Count, "unchanged", null));
                continue;
            }

            if (!byCode.TryGetValue(code, out var fabric)) error = $"Тканини {code} немає в «Тканини».";
            else if (keys.ContainsKey((code, colorKey))) error = "Цей рядок уже є на складі.";
            else error = Chain(row) ?? row.Movements
                .Where(x => x.Kind == nameof(FabricMovementKind.Issue) && TargetOf(x.OrderLegacyId, x.Position) is null)
                .Select(_ => "Видача до завдання, якого немає: спершу «Імпорт» завдань.").FirstOrDefault();
            lines.Add(new FabricStockImportLine(code, row.Color, row.Movements.Count, error is null ? "created" : "error",
                error));
            if (error is not null) continue;
            plans.Add((row, fabric!));
            keys[(code, colorKey)] = row.LegacyId;
            foreach (var m in row.Movements) fileMovements[m.LegacyId.Trim()] = m;
        }

        var issueErrors = new List<string>();
        var newIssues = input.Issues.Where(x => !movedIssues.Contains(x.LegacyId.Trim())).ToList();
        foreach (var issue in newIssues)
        {
            if (TargetOf(issue.OrderLegacyId, issue.Position) is null)
                issueErrors.Add($"Видача {issue.LegacyId}: немає завдання або позиції.");
            foreach (var l in issue.Lines.Where(l => !fileMovements.ContainsKey(l.MovementLegacyId.Trim())))
                issueErrors.Add($"Видача {issue.LegacyId}: рух {l.MovementLegacyId} не переноситься.");
        }

        var differences = await DifferencesAsync(input, newIssues, TargetOf, cancellationToken);
        var errors = lines.Count(x => x.Action == "error") + issueErrors.Count;
        var created = lines.Count(x => x.Action == "created");
        if (!command.DryRun && errors == 0 && (created > 0 || newIssues.Count > 0))
        {
            var objects = new Dictionary<string, FabricMovement>();
            foreach (var (row, fabric) in plans)
            {
                var first = row.Movements[0];
                var stock = new FabricStock(new StockFabric(fabric.Id, fabric.Code!, fabric.Name), row.Color, first.At,
                    row.LegacyId);
                foreach (var m in row.Movements)
                {
                    var kind = Enum.Parse<FabricMovementKind>(m.Kind);
                    var details = new FabricMovementDetails(m.OperationDate, m.Note, null, m.Supplier, m.Document,
                        kind == FabricMovementKind.Issue ? TargetOf(m.OrderLegacyId, m.Position) : null);
                    objects[m.LegacyId.Trim()] = stock.Transfer(kind, m.MetersBefore, m.Delta, details,
                        await NameAsync(m.ActorEmail, names, cancellationToken), m.At, m.LegacyId);
                }

                db.FabricStocks.Add(stock);
            }

            foreach (var row in newIssues)
            {
                var issue = new FabricIssue(TargetOf(row.OrderLegacyId, row.Position)!, row.IssuedOn, row.Note,
                    await NameAsync(row.ActorEmail, names, cancellationToken), row.At, row.LegacyId);
                foreach (var l in row.Lines)
                {
                    var movement = objects[l.MovementLegacyId.Trim()];
                    var purpose = Enum.Parse<FabricIssuePurpose>(l.Purpose);
                    var plan = purpose == FabricIssuePurpose.Additional || l.PlanFabricCode is null || l.PlanColor is null
                        ? null
                        : new FabricPlanKey(l.PlanFabricCode.Trim().ToUpperInvariant(), StockColor.Key(l.PlanColor));
                    issue.AddLine(plan is null ? FabricIssuePurpose.Additional : purpose, plan, l.PlannedMeters,
                        movement.Stock, movement, plan is null ? 0m : l.CoveredMeters);
                }

                db.FabricIssues.Add(issue);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new FabricStockImportResult(command.DryRun || errors > 0, created,
            lines.Count(x => x.Action == "unchanged"), errors, plans.Sum(x => x.Row.Movements.Count), newIssues.Count,
            differences, lines, issueErrors));
    }

    /// <summary>Every movement follows the one before it and they add up to the balance.</summary>
    private static string? Chain(FabricStockImportRow row)
    {
        var balance = 0m;
        foreach (var m in row.Movements)
        {
            if (m.MetersBefore != balance || m.MetersBefore + m.Delta != m.MetersAfter)
                return $"Рух {m.LegacyId} не сходиться: було {m.MetersBefore}, зміна {m.Delta}, стало {m.MetersAfter}.";
            if (!FabricStock.IsValidMeters(m.Delta) || !FabricStock.IsValidMeters(m.MetersAfter))
                return $"Рух {m.LegacyId}: забагато метрів.";
            balance = m.MetersAfter;
        }

        return balance == row.Meters ? null : $"Рухи дають {balance} м, а залишок {row.Meters} м.";
    }

    /// <summary>How many fabric and colour pairs reserve differently from the old app after these issues.</summary>
    private async Task<int> DifferencesAsync(FabricStockImportInput input, IReadOnlyList<FabricIssueImportRow> issues,
        Func<string?, int?, FabricMovementTarget?> targetOf, CancellationToken cancellationToken)
    {
        var covered = issues.SelectMany(x => x.Lines.Select(l => (Target: targetOf(x.OrderLegacyId, x.Position), Line: l)))
            .Where(x => x.Target is not null && x.Line.PlanFabricCode is not null && x.Line.PlanColor is not null &&
                        x.Line.Purpose != nameof(FabricIssuePurpose.Additional))
            .GroupBy(x => (x.Target!.ItemId, Code: x.Line.PlanFabricCode!.Trim().ToUpperInvariant(),
                Key: StockColor.Key(x.Line.PlanColor!)))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Line.CoveredMeters));
        var now = (await reservations.OpenAsync(null, cancellationToken))
            .GroupBy(x => (x.Part.FabricCode, x.Part.ColorKey))
            .ToDictionary(g => g.Key, g => g.Sum(x => Math.Max(0m,
                x.Reserved - covered.GetValueOrDefault((x.ItemId, x.Part.FabricCode, x.Part.ColorKey)))));
        var old = input.OldReservations.GroupBy(x => (x.FabricCode.Trim().ToUpperInvariant(), StockColor.Key(x.Color)))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Meters));
        return now.Keys.Union(old.Keys)
            .Count(k => Math.Abs(now.GetValueOrDefault(k) - old.GetValueOrDefault(k)) >= 0.01m);
    }

    private async Task<string> NameAsync(string? email, Dictionary<string, string> cache,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email)) return Transfer;
        if (cache.TryGetValue(email, out var name)) return name;
        name = (await staff.FindByEmailAsync(email.Trim(), cancellationToken))?.Name ?? Transfer;
        cache[email] = name;
        return name;
    }
}
