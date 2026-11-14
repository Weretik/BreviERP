using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Domain.Orders.Enums;

namespace Production.Application.Features.Orders.Commands.UploadProductionOrderFile;

public sealed class UploadProductionOrderFileHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionFileStorage storage, OrderCards cards,
    TimeProvider clock) : ICommandHandler<UploadProductionOrderFileCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(UploadProductionOrderFileCommand command,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        if (command.ItemId is { } itemId && order.Items.All(x => x.Id != itemId))
            return Result<ProductionOrderResponse>.NotFound(OrderMessages.ItemNotInOrder);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var name = command.DisplayName.Trim();
        var key = $"/production/orders/{order.Number}/{Guid.NewGuid():N}{SafeExtension(name)}";
        var url = await storage.UploadAsync(command.Content, key, command.ContentType, cancellationToken);
        var now = clock.GetUtcNow();
        var file = new ProductionOrderFile(order.Id, command.ItemId,
            Enum.Parse<ProductionFileKind>(command.Kind, ignoreCase: true), name, command.ContentType,
            command.SizeInBytes, url, key, actorName, now);
        db.OrderFiles.Add(file);
        order.RecordAbout(command.ItemId, "Файл додано", null, name, actorName, now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The CDN already has the file; remove it so it does not stay without a row.
            try { await storage.DeleteAsync(key, CancellationToken.None); } catch { /* best effort */ }
            throw;
        }

        return Result.Success(await cards.BuildAsync(order, command.Actor, cancellationToken));
    }

    /// <summary>A short Latin extension, so the CDN key stays safe whatever the file is called.</summary>
    private static string SafeExtension(string name)
    {
        var extension = Path.GetExtension(name).ToLowerInvariant();
        return extension.Length is > 1 and <= 8 && extension.Skip(1).All(char.IsAsciiLetterOrDigit) ? extension : "";
    }
}
