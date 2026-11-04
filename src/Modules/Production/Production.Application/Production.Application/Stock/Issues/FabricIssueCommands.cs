using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Application.Stock.Movements;
using Production.Domain.Orders;
using Production.Domain.Stock;

namespace Production.Application.Stock.Issues;

/// <summary>
/// «Видати»: one package of lines for an item (PRD-D30). A planned line covers its part up to the issued metres, a
/// replacement closes its part, an additional one covers nothing.
/// </summary>
public sealed record CreateFabricIssueCommand(ProductionActor Actor, int OrderId, int ItemId, CreateFabricIssueInput Input)
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
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.IssuedOn).NotEmpty().WithMessage("Вкажіть дату видачі.");
            RuleFor(x => x.Input.Note).MaximumLength(FabricIssue.NoteMaxLength);
            RuleFor(x => x.Input.Lines).NotNull().Must(x => x.Count is > 0 and <= FabricIssue.MaxLines)
                .WithMessage("Додайте від 1 до 20 рядків видачі.");
            RuleForEach(x => x.Input.Lines).ChildRules(line =>
            {
                line.RuleFor(x => x.Purpose).Must(x => Enum.TryParse<FabricIssuePurpose>(x, out var p) && Enum.IsDefined(p))
                    .WithMessage("Оберіть, для чого видача.");
                line.RuleFor(x => x.PlanColorKey).NotEmpty().WithMessage("Оберіть плановий колір.")
                    .When(x => x.Purpose != nameof(FabricIssuePurpose.Additional));
                line.RuleFor(x => x.PlanColorKey).Empty()
                    .When(x => x.Purpose == nameof(FabricIssuePurpose.Additional));
                line.RuleFor(x => x.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
                line.RuleFor(x => x.Color).NotEmpty().WithMessage("Вкажіть колір.").MaximumLength(StockColor.MaxLength);
                line.RuleFor(x => x.Meters).PositiveMeters();
            }).When(x => x.Input.Lines is not null);
        });
    }
}

public sealed class CreateFabricIssueCommandHandler(IProductionDbContext db, StockChanges changes,
    StockReservations reservations, ItemFabricCards cards)
    : ICommandHandler<CreateFabricIssueCommand, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(CreateFabricIssueCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var order = await db.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == command.OrderId && x.DeletedAt == null, cancellationToken);
        var item = order?.Items.FirstOrDefault(x => x.Id == command.ItemId);
        if (order is null || item is null) return Result<ProductionItemFabricResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanIssueFabric(order)) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);
        if (order.Version != input.OrderVersion) return Result<ProductionItemFabricResponse>.Conflict(OrderMessages.Stale);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);

        var plan = (await reservations.ItemAsync(order, item, cancellationToken))
            .ToDictionary(x => x.Part.ColorKey, x => (x.Part, Remaining: x.Reserved));
        var target = new FabricMovementTarget(order.Id, item.Id, order.Number, item.ModelName);
        var issue = new FabricIssue(target, input.IssuedOn, input.Note, actorName, changes.Now);
        var now = changes.Now;
        for (var i = 0; i < input.Lines.Count; i++)
        {
            var row = input.Lines[i];
            var purpose = Enum.Parse<FabricIssuePurpose>(row.Purpose);
            var fabric = await changes.FabricAsync(row.FabricId, cancellationToken);
            if (!fabric.IsSuccess) return fabric.Map(_ => default(ProductionItemFabricResponse)!);
            FabricPlanPart? part = null;
            if (purpose != FabricIssuePurpose.Additional)
            {
                if (!plan.TryGetValue(row.PlanColorKey!.Trim(), out var planned))
                    return Invalid(i, StockMessages.PlanPartNotFound);
                part = planned.Part;
            }

            var color = StockColor.Clean(row.Color);
            if (purpose == FabricIssuePurpose.Planned &&
                (fabric.Value.Code.Trim().ToUpperInvariant() != part!.FabricCode || StockColor.Key(color) != part.ColorKey))
                return Invalid(i, StockMessages.PlannedMismatch);

            var line = await changes.LineAsync(fabric.Value, color, cancellationToken);
            if (!FabricStock.IsValidMeters(line.Meters - row.Meters)) return Invalid(i, StockMessages.TooMuch);
            var covered = 0m;
            if (part is not null)
            {
                var remaining = plan[part.ColorKey].Remaining;
                covered = purpose == FabricIssuePurpose.Planned ? Math.Min(row.Meters, remaining) : remaining;
                plan[part.ColorKey] = (part, remaining - covered);
            }

            var movement = line.Move(FabricMovementKind.Issue, -row.Meters,
                new FabricMovementDetails(input.IssuedOn, input.Note, Target: target), actorName, now);
            issue.AddLine(purpose, part?.Key, part?.Meters, line, movement, covered);
        }

        db.FabricIssues.Add(issue);
        order.RecordAbout(item.Id, "Тканина", null, $"Видано {Meters(issue.Lines.Sum(x => x.Meters))} м", actorName, now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<ProductionItemFabricResponse>.Conflict([.. saved.Errors]);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }

    private static Result<ProductionItemFabricResponse> Invalid(int index, string message) =>
        Result<ProductionItemFabricResponse>.Invalid(new ValidationError($"Lines[{index}]", message));

    internal static string Meters(decimal value) => value.ToString("0.##", CultureInfo.GetCultureInfo("uk-UA"));
}

/// <summary>«Сторно видачі»: every line goes back to the stock and the plan is reserved again.</summary>
public sealed record ReverseFabricIssueCommand(ProductionActor Actor, int OrderId, int IssueId,
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

public sealed class ReverseFabricIssueCommandHandler(IProductionDbContext db, StockChanges changes, ItemFabricCards cards)
    : ICommandHandler<ReverseFabricIssueCommand, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(ReverseFabricIssueCommand command,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == command.OrderId && x.DeletedAt == null, cancellationToken);
        if (order is null) return Result<ProductionItemFabricResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanIssueFabric(order)) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);
        if (order.Version != command.Input.OrderVersion) return Result<ProductionItemFabricResponse>.Conflict(OrderMessages.Stale);
        var issue = await db.FabricIssues.Include(x => x.Lines).ThenInclude(x => x.Stock)
            .Include(x => x.Lines).ThenInclude(x => x.Movement)
            .SingleOrDefaultAsync(x => x.Id == command.IssueId && x.OrderId == order.Id, cancellationToken);
        if (issue is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.IssueNotFound);
        if (issue.IsReversed) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.IssueAlreadyReversed);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);

        var now = changes.Now;
        var details = new FabricMovementDetails(DateOnly.FromDateTime(now.UtcDateTime),
            $"Сторно видачі: {command.Input.Reason.Trim()}", Target: issue.Target);
        foreach (var line in issue.Lines.Where(x => !x.Movement.IsReversed))
            line.Stock.Reverse(line.Movement, details, actorName, now);
        issue.Reverse(command.Input.Reason, actorName, now);
        order.RecordAbout(issue.ItemId, "Тканина", null,
            $"Сторно видачі {CreateFabricIssueCommandHandler.Meters(issue.Lines.Sum(x => x.Meters))} м", actorName, now);
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<ProductionItemFabricResponse>.Conflict([.. saved.Errors]);
        var item = order.Items.First(x => x.Id == issue.ItemId);
        return Result.Success(await cards.BuildAsync(order, item, command.Actor, cancellationToken));
    }
}
