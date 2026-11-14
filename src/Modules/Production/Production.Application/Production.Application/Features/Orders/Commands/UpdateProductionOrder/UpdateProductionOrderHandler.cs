using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Colors.Shared;
using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Domain.Orders.Rules;
using Production.Application.Features.Orders.Shared.Responses;
using Production.Application.Features.Orders.Shared.Rules;
using BuildingBlocks.Application;

namespace Production.Application.Features.Orders.Commands.UpdateProductionOrder;

public sealed class UpdateProductionOrderHandler(IProductionDbContext db, IProductionStaffReader staff,
    OrderCards cards, EmbroideryLinks embroidery, ProductionColors colors, TimeProvider clock)
    : ICommandHandler<UpdateProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(UpdateProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var actor = command.Actor;
        var input = command.Input;
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        if (order.Version != input.Version) return Result<ProductionOrderResponse>.Conflict(OrderMessages.Stale);
        if (input.Items.Any(x => order.Items.All(item => item.Id != x.Id)) ||
            input.Items.Select(x => x.Id).Distinct().Count() != input.Items.Count)
            return Result<ProductionOrderResponse>.Invalid(new ValidationError(OrderMessages.ItemNotInOrder));

        if (input.Items.Any(x => x.Details.Quantity < order.ShippedQuantity(x.Id)))
            return Result<ProductionOrderResponse>.Invalid(new ValidationError(OrderMessages.QuantityBelowShipped));

        var actorName = await OrderActors.NameAsync(staff, actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var numbers = await embroidery.NumbersAsync(actor.UserId, input.Items.Select(x => x.Details), order,
            cancellationToken);
        if (!numbers.IsSuccess) return Result<ProductionOrderResponse>.Invalid(numbers.ValidationErrors.ToArray());
        var colorErrors = await colors.CheckAsync(input.Items.SelectMany(x => x.Details.Colors()),
            order.Items.SelectMany(x => x.AdditionalColors.Prepend(x.MainColor)), cancellationToken);
        if (colorErrors.Count > 0) return Result<ProductionOrderResponse>.Invalid(colorErrors.ToArray());

        var norms = await OrderNorms.ReadAsync(db, order.Items.Select(x => x.ModelCode), cancellationToken);
        var updates = input.Items.ToDictionary(x => x.Id, x =>
        {
            var item = order.Items.Single(i => i.Id == x.Id);
            var details = x.Details.ToDetails(numbers.Value);
            var colors = 1 + details.AdditionalColors.Count(c => !string.IsNullOrWhiteSpace(c));
            var plan = FabricPlan.Meters(item.ModelCode is null ? null : norms.GetValueOrDefault(item.ModelCode),
                colors, details.Quantity);
            return new ItemUpdate(details, plan, actor.SeesSewingRate,
                x.SewingRate is { } rate ? decimal.Round(rate, 2) : null);
        });
        var updated = order.Update(new OrderHeader(input.ClientName, input.OrderDate, input.Comment), updates, actorName,
            clock.GetUtcNow());
        if (updated.IsFailure) return updated.Error!.ToResult<ProductionOrderResponse>();
        if (updated.Value) await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(order, actor, cancellationToken));
    }
}
