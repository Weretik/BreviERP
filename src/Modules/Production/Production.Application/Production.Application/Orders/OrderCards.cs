using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Embroidery;
using Production.Domain.Orders;
using Production.Domain.Settings;

namespace Production.Application.Orders;

/// <summary>Builds the order card: the order with its files and the Telegram mark.</summary>
public sealed class OrderCards(IProductionDbContext db, IProductionTelegram telegram, EmbroideryLinks embroidery)
{
    /// <summary>Orders with what the card needs; tracked, so a handler can change them.</summary>
    public IQueryable<ProductionOrder> Orders =>
        db.Orders.Include(x => x.Items).Include(x => x.Shipments).Where(x => x.DeletedAt == null);

    public async Task<ProductionOrderResponse> BuildAsync(ProductionOrder order, ProductionActor actor,
        CancellationToken cancellationToken)
    {
        var files = await db.OrderFiles.AsNoTracking()
            .Where(x => x.OrderId == order.Id && x.DeletedAt == null).ToListAsync(cancellationToken);
        var delivery = await db.TelegramDeliveries.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);
        var embroideries = await embroidery.ReadAsync(order, actor.UserId, cancellationToken);
        return order.ToResponse(actor, files, delivery, await TelegramReadyAsync(cancellationToken), embroideries);
    }

    public async Task<bool> TelegramReadyAsync(CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId, cancellationToken);
        return telegram.IsConfigured && settings is { CanSendTelegram: true };
    }
}
