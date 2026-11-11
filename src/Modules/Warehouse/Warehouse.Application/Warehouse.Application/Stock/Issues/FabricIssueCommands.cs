using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Application.Access;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Issues;

/// <summary>
/// «Видача» by actual cutting (TZ §7): products cut now and the real fabric, one or more pairs of the stock. The server
/// compares each planned part with P × n / N once, calls a line «планова» when it is the planned pair and «заміна»
/// otherwise, and spends each pair in order over its stock lines. One confirmed form is one package and one operation.
/// </summary>
public sealed record CreateFabricIssueCommand(WarehouseActor Actor, int OrderId, int ItemId, CreateFabricIssueInput Input)
    : ICommand<Result<ProductionItemFabricResponse>>;

public sealed class CreateFabricIssueCommandValidator : AbstractValidator<CreateFabricIssueCommand>
{
    public CreateFabricIssueCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.PlanVersion).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.IssuedOn).OperationDate();
            RuleFor(x => x.Input.IssuedQuantity).GreaterThan(0).WithMessage(StockMessages.IssuedQuantity);
            RuleFor(x => x.Input.Note).MaximumLength(FabricIssue.NoteMaxLength);
            RuleFor(x => x.Input.Lines).NotNull().Must(x => x.Count is > 0 and <= FabricIssue.MaxLines)
                .WithMessage("Додайте від 1 до 20 рядків видачі.");
            RuleForEach(x => x.Input.Lines).ChildRules(line =>
            {
                line.RuleFor(x => x.FabricCode).NotEmpty().WithMessage("Оберіть тканину зі складу.")
                    .MaximumLength(FabricStock.CodeMaxLength);
                line.RuleFor(x => x.Color).NotEmpty().WithMessage("Оберіть колір зі складу.")
                    .MaximumLength(StockColor.MaxLength);
                line.RuleFor(x => x.ExpectedVersion).MaximumLength(64);
                line.RuleFor(x => x.Meters).PositiveMeters();
                line.RuleFor(x => x.PlanColorKey).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.PlanFabricCode))
                    .WithMessage("Оберіть плановий колір.");
                line.RuleFor(x => x.Layer).Must(x => x is null || Enum.TryParse<FabricLayer>(x, out var l) && Enum.IsDefined(l))
                    .WithMessage(StockMessages.Layer);
                line.RuleFor(x => x.Note).MaximumLength(FabricIssueLine.NoteMaxLength);
            }).When(x => x.Input.Lines is not null);
        });
    }
}

public sealed class CreateFabricIssueCommandHandler(IWarehouseProductionReader production, IWarehouseDbContext db,
    StockChanges changes, StockPairs pairs, StockOperations operations, ItemPlans plans, ItemFabricCards cards)
    : ICommandHandler<CreateFabricIssueCommand, Result<ProductionItemFabricResponse>>
{
    public const string Kind = "order-issue";

    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(CreateFabricIssueCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var item = await production.ItemAsync(command.OrderId, command.ItemId, cancellationToken);
        if (item is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.ItemNotFound);
        if (!command.Actor.CanMoveStock) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind,
            new { command.OrderId, command.ItemId, Input = input with { OrderVersion = 0, PlanVersion = 0,
                Lines = input.Lines.Select(x => x with { ExpectedVersion = null }).ToList() } });
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay)
            return replay.IsSuccess
                ? Result.Success(await cards.BuildAsync(item, command.Actor, cancellationToken))
                : Result<ProductionItemFabricResponse>.Conflict([.. replay.Errors]);
        if (!item.IsOpen) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderClosed);
        if (item.OrderVersion != input.OrderVersion) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderStale);
        if (input.IssuedQuantity > Math.Max(1, item.Quantity)) return Invalid("IssuedQuantity", StockMessages.IssuedQuantity);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);

        var plan = await plans.TrackedAsync(item.ItemId, cancellationToken);
        if ((plan?.Version ?? 0) != input.PlanVersion) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.PlanStale);
        var now = pairs.Now;
        plan ??= plans.Save(item, actorName, now);
        var planned = ItemPlans.Apply(item, plan);
        var covered = (await db.FabricIssues.AsNoTracking().Include(x => x.Lines)
                .Where(x => x.ItemId == item.ItemId && x.ReversedAt == null).ToListAsync(cancellationToken))
            .SelectMany(x => x.Lines).Where(x => x.Plan is not null)
            .GroupBy(x => x.Plan!).ToDictionary(g => g.Key, g => g.Sum(x => x.CoveredMeters));

        // Every pair is loaded and checked before anything is spent; lines of one pair spend it one after another.
        var forms = new List<(FabricIssueLineInput Row, StockPair Pair, FabricItemPlanPart? Part)>();
        for (var i = 0; i < input.Lines.Count; i++)
        {
            var row = input.Lines[i];
            var pair = await pairs.LoadAsync(row.FabricCode, row.Color, cancellationToken);
            if (pair.Lines.Count == 0) return Invalid($"Lines[{i}]", StockMessages.NotHeld);
            if (row.ExpectedVersion is { Length: > 0 } version && version != pair.Version)
                return Result<ProductionItemFabricResponse>.Conflict(StockMessages.PairStale);
            FabricItemPlanPart? part = null;
            if (!string.IsNullOrWhiteSpace(row.PlanFabricCode))
            {
                part = plan.Parts.FirstOrDefault(x => x.FabricCode == FabricAliases.Normalize(row.PlanFabricCode) &&
                                                      x.ColorKey == row.PlanColorKey!.Trim());
                if (part is null) return Invalid($"Lines[{i}]", StockMessages.PlanPartNotFound);
            }

            forms.Add((row, forms.FirstOrDefault(x => x.Pair.Code == pair.Code && x.Pair.ColorKey == pair.ColorKey).Pair ?? pair,
                part));
        }

        var target = item.Target;
        var issue = new FabricIssue(target, input.IssuedOn, input.Note, actorName, now, issuedQuantity: input.IssuedQuantity,
            operationId: input.OperationId, planVersion: plan.Version);
        var left = forms.Where(x => x.Part is not null).Select(x => x.Part!).Distinct().ToDictionary(x => x,
            x => Math.Min(FabricItemPlan.ComparisonPlan(x.Meters, input.IssuedQuantity, plan.Quantity),
                Math.Max(0m, x.Meters - covered.GetValueOrDefault(x.Key))));
        var touched = new List<FabricStock>();
        for (var i = 0; i < forms.Count; i++)
        {
            var (row, pair, part) = forms[i];
            var details = new FabricMovementDetails(input.IssuedOn, row.Note ?? input.Note, Target: target,
                OperationId: input.OperationId);
            var spent = pair.Issue(row.Meters, details, actorName, now);
            if (!spent.IsDone) return Invalid($"Lines[{i}]", StockMessages.TooMuch);
            touched.AddRange(spent.Lines);
            var purpose = part is null ? FabricIssuePurpose.Additional
                : FabricAliases.Canonical(part.FabricCode) == pair.Code && part.ColorKey == pair.ColorKey
                    ? FabricIssuePurpose.Planned
                    : FabricIssuePurpose.Replacement;
            var share = 0m;
            if (part is not null)
            {
                // The coverage of a part goes to its lines in order, each up to its metres, the rest to the last one.
                var last = forms.Skip(i + 1).All(x => x.Part != part);
                share = last ? left[part] : Math.Min(row.Meters, left[part]);
                left[part] -= share;
            }

            var layer = part?.Layer ?? (row.Layer is null ? null : Enum.Parse<FabricLayer>(row.Layer));
            issue.AddLine(purpose, part?.Key,
                part is null ? null : FabricItemPlan.ComparisonPlan(part.Meters, input.IssuedQuantity, plan.Quantity),
                spent.Movements, share, layer, row.Note);
        }

        db.FabricIssues.Add(issue);
        var saved = await SaveAsync(command.Actor, input.OperationId, hash, touched, issue, forms[0].Pair, cancellationToken);
        if (saved is not null) return saved;
        await production.RecordFabricAsync(item.OrderId, item.ItemId,
            $"Видано {Meters(issue.Lines.Sum(x => x.Meters))} м на {input.IssuedQuantity} шт.", actorName, cancellationToken);
        return Result.Success(await cards.BuildAsync(item, command.Actor, cancellationToken));
    }

    /// <summary>Restates every touched line, records the operation and saves; null when saved.</summary>
    private async Task<Result<ProductionItemFabricResponse>?> SaveAsync(WarehouseActor actor, Guid operationId,
        string hash, IReadOnlyCollection<FabricStock> touched, FabricIssue issue, StockPair first,
        CancellationToken cancellationToken)
    {
        var lines = touched.Distinct().ToList();
        var ids = lines.Select(x => x.Id).ToArray();
        var savedMovements = await db.FabricMovements.Where(x => ids.Contains(x.StockId)).ToListAsync(cancellationToken);
        foreach (var line in lines) line.Restate(savedMovements.Where(x => x.StockId == line.Id).Concat(line.Movements));
        db.StockOperations.Add(new StockOperation(operationId, Kind, hash, actor.UserId, first.Code, first.ColorKey,
            first.Lines[0].Color, first.Meters, pairs.Now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException)
        {
            if (await operations.ReplayAsync(operationId, hash, cancellationToken) is { IsSuccess: true })
                return null;
            return Result<ProductionItemFabricResponse>.Conflict(StockMessages.PairStale);
        }
    }

    private static Result<ProductionItemFabricResponse> Invalid(string field, string message) =>
        Result<ProductionItemFabricResponse>.Invalid(new ValidationError(field, message));

    internal static string Meters(decimal value) => value.ToString("0.##", CultureInfo.GetCultureInfo("uk-UA"));
}

/// <summary>«Сторно видачі»: every line goes back to the stock and the plan is reserved again.</summary>
public sealed record ReverseFabricIssueCommand(WarehouseActor Actor, int OrderId, int IssueId,
    ReverseFabricIssueInput Input) : ICommand<Result<ProductionItemFabricResponse>>;

public sealed class ReverseFabricIssueCommandValidator : AbstractValidator<ReverseFabricIssueCommand>
{
    public ReverseFabricIssueCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.IssueId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.Reason).Reason();
        });
    }
}

public sealed class ReverseFabricIssueCommandHandler(IWarehouseProductionReader production, IWarehouseDbContext db,
    StockChanges changes, ItemFabricCards cards)
    : ICommandHandler<ReverseFabricIssueCommand, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(ReverseFabricIssueCommand command,
        CancellationToken cancellationToken)
    {
        var issue = await db.FabricIssues.Include(x => x.Lines).ThenInclude(x => x.Movements).ThenInclude(x => x.Stock)
            .SingleOrDefaultAsync(x => x.Id == command.IssueId && x.OrderId == command.OrderId, cancellationToken);
        if (issue is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.IssueNotFound);
        var item = await production.ItemAsync(issue.OrderId, issue.ItemId, cancellationToken);
        if (item is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.ItemNotFound);
        if (!command.Actor.CanMoveStock) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);
        if (!item.IsOpen) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderClosed);
        if (item.OrderVersion != command.Input.OrderVersion)
            return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderStale);
        if (issue.IsReversed) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.IssueAlreadyReversed);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);

        var now = changes.Now;
        var details = new FabricMovementDetails(DateOnly.FromDateTime(now.UtcDateTime),
            $"Сторно видачі: {command.Input.Reason.Trim()}", Target: issue.Target);
        foreach (var movement in issue.Lines.SelectMany(x => x.Movements).Where(x => !x.IsReversed && !x.IsVoided))
            movement.Stock.Reverse(movement, details, actorName, now);
        issue.Reverse(command.Input.Reason, actorName, now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<ProductionItemFabricResponse>.Conflict([.. saved.Errors]);
        await production.RecordFabricAsync(issue.OrderId, issue.ItemId,
            $"Сторно видачі {CreateFabricIssueCommandHandler.Meters(issue.Lines.Sum(x => x.Meters))} м", actorName,
            cancellationToken);
        return Result.Success(await cards.BuildAsync(item, command.Actor, cancellationToken));
    }
}
