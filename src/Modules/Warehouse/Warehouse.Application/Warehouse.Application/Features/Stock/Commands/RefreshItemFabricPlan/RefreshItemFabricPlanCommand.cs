using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;

/// <summary>
/// «Оновити план»: the norm of today replaces the saved plan of an item (TZ §5.2). Issues already made keep the plan
/// they were compared with; the change is written to the item history.
/// </summary>
public sealed record RefreshItemFabricPlanCommand(WarehouseActor Actor, int OrderId, int ItemId,
    RefreshItemFabricPlanInput Input) : ICommand<Result<ProductionItemFabricResponse>>;

public sealed class RefreshItemFabricPlanValidator : AbstractValidator<RefreshItemFabricPlanCommand>
{
    public RefreshItemFabricPlanValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OrderVersion).GreaterThan(0);
            RuleFor(x => x.Input.PlanVersion).GreaterThan(0);
        });
    }
}

public sealed class RefreshItemFabricPlanHandler(IWarehouseProductionReader production, IWarehouseDbContext db,
    StockChanges changes, ItemPlans plans, ItemFabricCards cards)
    : ICommandHandler<RefreshItemFabricPlanCommand, Result<ProductionItemFabricResponse>>
{
    public async ValueTask<Result<ProductionItemFabricResponse>> Handle(RefreshItemFabricPlanCommand command,
        CancellationToken cancellationToken)
    {
        var item = await production.ItemAsync(command.OrderId, command.ItemId, cancellationToken);
        if (item is null) return Result<ProductionItemFabricResponse>.NotFound(StockMessages.ItemNotFound);
        if (!command.Actor.CanMoveStock) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);
        if (!item.IsOpen) return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderClosed);
        if (item.OrderVersion != command.Input.OrderVersion)
            return Result<ProductionItemFabricResponse>.Conflict(StockMessages.OrderStale);
        var plan = await plans.TrackedAsync(item.ItemId, cancellationToken);
        if (plan is null || plan.Version != command.Input.PlanVersion)
            return Result<ProductionItemFabricResponse>.Conflict(StockMessages.PlanStale);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<ProductionItemFabricResponse>.Forbidden(StockMessages.CannotIssue);

        var before = Describe(plan.Quantity, plan.Parts);
        var parts = ItemPlans.Parts(item).ToList();
        var after = Describe(Math.Max(1, item.Quantity), parts);
        if (before == after) return Result<ProductionItemFabricResponse>.Invalid(
            new ValidationError("PlanVersion", StockMessages.NothingToRefresh));
        plan.Refresh(Math.Max(1, item.Quantity), parts, actorName, changes.Now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<ProductionItemFabricResponse>.Conflict(StockMessages.PlanStale);
        }

        await production.RecordFabricAsync(item.OrderId, item.ItemId, $"План тканини оновлено: було {before}; стало {after}",
            actorName, cancellationToken);
        return Result.Success(await cards.BuildAsync(item, command.Actor, cancellationToken));
    }

    private static string Describe(int quantity, IEnumerable<FabricItemPlanPart> parts)
    {
        var text = string.Join(", ", parts.OrderBy(x => x.FabricCode).ThenBy(x => x.ColorKey)
            .Select(x => $"{x.FabricCode} {x.Color} {CreateFabricIssueHandler.Meters(x.Meters)} м"));
        return $"{quantity} шт.: {(text.Length == 0 ? "без плану" : text)}";
    }
}
