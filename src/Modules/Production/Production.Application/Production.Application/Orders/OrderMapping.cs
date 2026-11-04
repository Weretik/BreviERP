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

    /// <param name="telegramReady">The workshop group is set and the bot has its token.</param>
    public static ProductionOrderResponse ToResponse(this ProductionOrder order, ProductionActor actor,
        IReadOnlyList<ProductionOrderFile> files, ProductionTelegramDelivery? delivery, bool telegramReady)
    {
        var canEdit = actor.CanEdit(order);
        var canTrack = actor.CanTrack(order);
        return new ProductionOrderResponse(
            order.Id, order.Number, order.Status.ToString(), order.ClientName, order.QuoteId, order.QuoteNumber,
            order.ManagerName, order.OrderDate, order.Comment,
            actor.SeesReportPrice ? order.PriceTier?.ToString() : null,
            order.LegacyId is not null, order.CopiedFromOrderId, order.CreatedAt, order.CreatedByName,
            order.UpdatedAt, order.Version, order.CancelReason,
            new ProductionOrderPermissions(canEdit, actor.CanCreate && !order.IsDeleted, actor.CanDelete,
                actor.SeesSewingRate, actor.SeesReportPrice, canTrack, canTrack && order.IsFullyShipped, canEdit,
                actor.CanReopen(order), canEdit,
                canEdit && delivery is not { CanSend: false } && telegramReady),
            order.Items.OrderBy(x => x.Position).Select(x => x.ToResponse(order, actor)).ToArray(),
            files.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => x.ToResponse()).ToArray(),
            new ProductionTelegramResponse(
                (delivery?.Status ?? TelegramDeliveryStatus.NotSent).ToString(), delivery?.SentAt,
                delivery?.SentByName, delivery?.LastError, telegramReady));
    }

    public static ProductionFileResponse ToResponse(this ProductionOrderFile file) => new(
        file.Id, file.ItemId, file.Kind.ToString(), file.Name, file.ContentType, file.Bytes, file.Url,
        file.StorageKey is null, file.CreatedByName, file.CreatedAt);

    public static ProductionItemResponse ToResponse(this ProductionItem item, ProductionOrder order,
        ProductionActor actor) => new(
        item.Id, item.Position, item.ModelCode, item.ModelName, item.FabricCode, item.FabricName, item.Quantity,
        item.DueDate, item.MainColor, item.AdditionalColors.ToArray(), item.SizesText, item.TechnicalChanges,
        item.Comment,
        item.Applications.Select(x => new ItemApplicationResponse(x.Kind.ToString(), x.Place, x.Description, x.Color))
            .ToArray(),
        item.PlannedFabricMeters,
        actor.SeesSewingRate ? item.SewingRate : null,
        actor.SeesReportPrice ? item.ReportUnitPrice : null,
        item.Stages.Select(x => x.ToString()).ToArray(),
        item.CurrentStage.ToString(),
        order.ShippedQuantity(item.Id),
        order.Shipments.Where(x => x.ItemId == item.Id).OrderBy(x => x.ShippedOn).ThenBy(x => x.Id)
            .Select(x => new ProductionShipmentResponse(x.Id, x.Quantity, x.ShippedOn, x.Comment, x.CreatedByName,
                x.CreatedAt))
            .ToArray());

    public static ProductionOrderChangeResponse ToResponse(this ProductionOrderChange change) => new(
        change.Id, change.ItemId, change.ItemName, change.Field, change.Before, change.After, change.ActorName,
        change.At);

    public static FabricNormInput? ToNormInput(this ModelFabricNorm? norm) => norm?.Values.ToInput();
}
