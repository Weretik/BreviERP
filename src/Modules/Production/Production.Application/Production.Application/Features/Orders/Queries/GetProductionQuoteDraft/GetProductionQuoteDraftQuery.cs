using Production.Application.Features.FabricNorms.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;

/// <summary>The КП for the order form: its sewing items with fabric variants, norms and, for the workshop, rates.</summary>
public sealed record GetProductionQuoteDraftQuery(ProductionActor Actor, long QuoteId)
    : IQuery<Result<ProductionQuoteDraftResponse>>;

public sealed class GetProductionQuoteDraftValidator : AbstractValidator<GetProductionQuoteDraftQuery>
{
    public GetProductionQuoteDraftValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.QuoteId).GreaterThan(0);
    }
}

public sealed class GetProductionQuoteDraftHandler(
    Abstractions.IProductionQuoteReader quotes, QuoteItemResolver resolver,
    Abstractions.IProductionEmbroideryReader embroideries)
    : IQueryHandler<GetProductionQuoteDraftQuery, Result<ProductionQuoteDraftResponse>>
{
    public async ValueTask<Result<ProductionQuoteDraftResponse>> Handle(GetProductionQuoteDraftQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.CanCreate) return Result<ProductionQuoteDraftResponse>.Forbidden(OrderMessages.CannotCreate);
        var quote = await quotes.ReadAsync(query.Actor.QuoteReader, query.QuoteId, cancellationToken);
        if (quote is null) return Result<ProductionQuoteDraftResponse>.NotFound(OrderMessages.QuoteNotFound);
        if (!quote.ForWorkshop) return Result<ProductionQuoteDraftResponse>.Conflict(OrderMessages.QuoteNotForWorkshop);

        var items = new List<ProductionQuoteDraftItem>(quote.Items.Count);
        foreach (var item in quote.Items)
        {
            var resolved = await resolver.ResolveAsync(item, cancellationToken);
            items.Add(new ProductionQuoteDraftItem(item.Id, item.Name, resolved.ModelCode, resolved.Norm?.ToInput(),
                resolved.Options.Select(x => new ProductionQuoteDraftOption(x.Option.Id, x.Option.Title, x.FabricCode,
                    x.FabricName, x.Option.Retail, x.Option.Wholesale, x.Option.Large,
                    query.Actor.SeesSewingRate ? x.SewingRate : null)).ToArray()));
        }

        var rows = quote.Embroideries ?? [];
        var calculations = rows.Count == 0
            ? new Dictionary<long, ProductionEmbroideryResponse>()
            : await embroideries.ReadAsync(query.Actor.UserId, rows.Select(x => x.CalculationId).Distinct().ToArray(),
                cancellationToken);
        var embroidery = rows.Where(x => calculations.ContainsKey(x.CalculationId))
            .Select(x => new ProductionQuoteDraftEmbroidery(x.QuoteItemId, calculations[x.CalculationId].ToChoice()))
            .ToArray();
        return Result.Success(new ProductionQuoteDraftResponse(quote.Id, quote.Number, quote.ClientName,
            quote.ResponsibleName, items, embroidery));
    }
}
