using Production.Application.Abstractions;
using Production.Application.Colors;
using Production.Application.Contracts;
using Production.Application.Orders.Embroidery;
using Production.Application.Orders.QuoteSources;
using Production.Domain.Orders;

namespace Production.Application.Orders.CreateOrder;

public sealed class CreateProductionOrderCommandHandler(
    IProductionDbContext db, IProductionQuoteReader quotes, IProductionStaffReader staff, QuoteItemResolver resolver,
    OrderCards cards, EmbroideryLinks embroidery, ProductionColors colors, TimeProvider clock)
    : ICommandHandler<CreateProductionOrderCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(CreateProductionOrderCommand command,
        CancellationToken cancellationToken)
    {
        var actor = command.Actor;
        var input = command.Input;
        if (!actor.CanCreate) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotCreate);

        var existing = await cards.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CreateRequestId == input.RequestId, cancellationToken);
        if (existing is not null) return Result.Success(await cards.BuildAsync(existing, actor, cancellationToken));

        var quote = await quotes.ReadAsync(actor.QuoteReader, input.QuoteId, cancellationToken);
        if (quote is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.QuoteNotFound);
        if (!quote.ForWorkshop) return Result<ProductionOrderResponse>.Conflict(OrderMessages.QuoteNotForWorkshop);
        if (input.Items.Select(x => x.QuoteOptionId).Distinct().Count() != input.Items.Count)
            return Result<ProductionOrderResponse>.Invalid(new ValidationError(OrderMessages.DuplicateItem));

        var actorName = await OrderActors.NameAsync(staff, actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var numbers = await embroidery.NumbersAsync(actor.UserId, input.Items.Select(x => x.Details), null,
            cancellationToken);
        if (!numbers.IsSuccess) return Result<ProductionOrderResponse>.Invalid(numbers.ValidationErrors.ToArray());
        var colorErrors = await colors.CheckAsync(input.Items.SelectMany(x => x.Details.Colors()), [],
            cancellationToken);
        if (colorErrors.Count > 0) return Result<ProductionOrderResponse>.Invalid(colorErrors.ToArray());

        var tier = Enum.Parse<ReportPriceTier>(input.PriceTier, ignoreCase: true);
        var items = new List<ProductionItem>(input.Items.Count);
        foreach (var line in input.Items)
        {
            var quoteItem = quote.Items.FirstOrDefault(x => x.Id == line.QuoteItemId);
            var option = quoteItem?.Options.FirstOrDefault(x => x.Id == line.QuoteOptionId);
            if (quoteItem is null || option is null)
                return Result<ProductionOrderResponse>.Invalid(new ValidationError(OrderMessages.QuoteItemNotFound));

            var resolved = await resolver.ResolveAsync(quoteItem, cancellationToken);
            var variant = resolved.Options.Single(x => x.Option.Id == option.Id);
            var details = line.Details.ToDetails(numbers.Value);
            var source = new ItemSource(quoteItem.Id, option.Id, quoteItem.ProductId, option.ArticleId,
                resolved.ModelCode, quoteItem.Name, option.FabricId, variant.FabricCode, variant.FabricName,
                variant.SewingRate, variant.Price(tier));
            var colors = 1 + details.AdditionalColors.Count(x => !string.IsNullOrWhiteSpace(x));
            items.Add(new ProductionItem(items.Count + 1, source, details,
                FabricPlan.Meters(resolved.Norm, colors, details.Quantity)));
        }

        var now = clock.GetUtcNow();
        var order = new ProductionOrder(await OrderNumbers.NextAsync(db, cancellationToken),
            new OrderOrigin(quote.Id, quote.Number, quote.CustomerId, quote.ResponsibleUserId, quote.ResponsibleName,
                input.RequestId),
            new OrderHeader(input.ClientName, input.OrderDate, input.Comment), tier, items, actorName, now);
        order.Record("Створено", $"З КП {quote.Number}", actorName, now);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        db.TelegramDeliveries.Add(new ProductionTelegramDelivery(order.Id));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(order, actor, cancellationToken));
    }
}
