using Production.Application.Contracts;
using Production.Application.FabricNorms;
using Production.Application.Orders.Access;

namespace Production.Application.Orders.QuoteSources;

/// <summary>The КП for the order form: its sewing items with fabric variants, norms and, for the workshop, rates.</summary>
public sealed record GetProductionQuoteDraftQuery(ProductionActor Actor, long QuoteId)
    : IQuery<Result<ProductionQuoteDraftResponse>>;

public sealed class GetProductionQuoteDraftQueryValidator : AbstractValidator<GetProductionQuoteDraftQuery>
{
    public GetProductionQuoteDraftQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.QuoteId).GreaterThan(0);
    }
}

public sealed class GetProductionQuoteDraftQueryHandler(
    Abstractions.IProductionQuoteReader quotes, QuoteItemResolver resolver)
    : IQueryHandler<GetProductionQuoteDraftQuery, Result<ProductionQuoteDraftResponse>>
{
    public async ValueTask<Result<ProductionQuoteDraftResponse>> Handle(GetProductionQuoteDraftQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.CanCreate) return Result<ProductionQuoteDraftResponse>.Forbidden(OrderMessages.CannotCreate);
        var quote = await quotes.ReadAsync(query.Actor.QuoteReader, query.QuoteId, cancellationToken);
        if (quote is null) return Result<ProductionQuoteDraftResponse>.NotFound(OrderMessages.QuoteNotFound);
        if (!quote.IsSent || quote.IsArchived) return Result<ProductionQuoteDraftResponse>.Conflict(OrderMessages.QuoteNotSent);

        var items = new List<ProductionQuoteDraftItem>(quote.Items.Count);
        foreach (var item in quote.Items)
        {
            var resolved = await resolver.ResolveAsync(item, cancellationToken);
            items.Add(new ProductionQuoteDraftItem(item.Id, item.Name, resolved.ModelCode, resolved.Norm?.ToInput(),
                resolved.Options.Select(x => new ProductionQuoteDraftOption(x.Option.Id, x.Option.Title, x.FabricCode,
                    x.FabricName, x.Option.Retail, x.Option.Wholesale, x.Option.Large,
                    query.Actor.SeesSewingRate ? x.SewingRate : null)).ToArray()));
        }

        return Result.Success(new ProductionQuoteDraftResponse(quote.Id, quote.Number, quote.ClientName,
            quote.ResponsibleName, items));
    }
}
