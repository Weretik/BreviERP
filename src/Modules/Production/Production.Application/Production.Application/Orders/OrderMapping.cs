using Production.Application.Contracts;
using Production.Application.FabricNorms;
using Production.Application.Orders.Access;
using Production.Domain.FabricNorms;
using Production.Domain.Orders;

namespace Production.Application.Orders;

internal static class OrderMapping
{
    public static ItemDetails ToDetails(this ProductionItemDetailsInput input) => new(
        input.Quantity, input.DueDate, input.MainColor, input.AdditionalColors ?? [], input.SizesText,
        input.TechnicalChanges, input.Comment,
        (input.Applications ?? []).Select(x => ItemApplication.Create(
            Enum.Parse<ItemApplicationKind>(x.Kind, ignoreCase: true), x.Place, x.Description, x.Color)).ToArray());

    public static ProductionOrderResponse ToResponse(this ProductionOrder order, ProductionActor actor) => new(
        order.Id, order.Number, order.Status.ToString(), order.ClientName, order.QuoteId, order.QuoteNumber,
        order.ManagerName, order.OrderDate, order.Comment,
        actor.SeesReportPrice ? order.PriceTier?.ToString() : null,
        order.LegacyId is not null, order.CopiedFromOrderId, order.CreatedAt, order.CreatedByName, order.UpdatedAt,
        order.Version,
        new ProductionOrderPermissions(actor.CanEdit(order), actor.CanCreate && !order.IsDeleted, actor.CanDelete,
            actor.SeesSewingRate, actor.SeesReportPrice),
        order.Items.OrderBy(x => x.Position).Select(x => x.ToResponse(actor)).ToArray());

    public static ProductionItemResponse ToResponse(this ProductionItem item, ProductionActor actor) => new(
        item.Id, item.Position, item.ModelCode, item.ModelName, item.FabricCode, item.FabricName, item.Quantity,
        item.DueDate, item.MainColor, item.AdditionalColors.ToArray(), item.SizesText, item.TechnicalChanges,
        item.Comment,
        item.Applications.Select(x => new ItemApplicationResponse(x.Kind.ToString(), x.Place, x.Description, x.Color))
            .ToArray(),
        item.PlannedFabricMeters,
        actor.SeesSewingRate ? item.SewingRate : null,
        actor.SeesReportPrice ? item.ReportUnitPrice : null);

    public static ProductionOrderChangeResponse ToResponse(this ProductionOrderChange change) => new(
        change.Id, change.ItemId, change.ItemName, change.Field, change.Before, change.After, change.ActorName,
        change.At);

    public static FabricNormInput? ToNormInput(this ModelFabricNorm? norm) => norm?.Values.ToInput();
}
