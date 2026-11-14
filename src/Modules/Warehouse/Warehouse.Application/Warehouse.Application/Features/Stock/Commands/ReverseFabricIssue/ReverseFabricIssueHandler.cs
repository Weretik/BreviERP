using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;

public sealed class ReverseFabricIssueHandler(IWarehouseProductionReader production, IWarehouseDbContext db,
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
            $"Сторно видачі {CreateFabricIssueHandler.Meters(issue.Lines.Sum(x => x.Meters))} м", actorName,
            cancellationToken);
        return Result.Success(await cards.BuildAsync(item, command.Actor, cancellationToken));
    }
}
