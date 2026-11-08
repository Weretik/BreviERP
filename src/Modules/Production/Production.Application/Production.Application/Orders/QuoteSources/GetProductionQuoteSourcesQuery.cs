using Production.Application.Contracts;
using Production.Application.Orders.Access;

namespace Production.Application.Orders.QuoteSources;

/// <summary>КП «під запит» the person may pass to the workshop, with how many orders each already has.</summary>
public sealed record GetProductionQuoteSourcesQuery(ProductionActor Actor, string? Search)
    : IQuery<Result<IReadOnlyList<ProductionQuoteSourceResponse>>>;

public sealed class GetProductionQuoteSourcesQueryValidator : AbstractValidator<GetProductionQuoteSourcesQuery>
{
    public GetProductionQuoteSourcesQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(200);
    }
}

public sealed class GetProductionQuoteSourcesQueryHandler(
    Abstractions.IProductionDbContext db, Abstractions.IProductionQuoteReader quotes)
    : IQueryHandler<GetProductionQuoteSourcesQuery, Result<IReadOnlyList<ProductionQuoteSourceResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductionQuoteSourceResponse>>> Handle(
        GetProductionQuoteSourcesQuery query, CancellationToken cancellationToken)
    {
        if (!query.Actor.CanCreate)
            return Result<IReadOnlyList<ProductionQuoteSourceResponse>>.Forbidden(OrderMessages.CannotCreate);
        var sent = await quotes.ListForWorkshopAsync(query.Actor.QuoteReader, query.Search, cancellationToken);
        var ids = sent.Select(x => (long?)x.Id).ToArray();
        var counts = await db.Orders.AsNoTracking()
            .Where(x => x.DeletedAt == null && ids.Contains(x.QuoteId))
            .GroupBy(x => x.QuoteId).Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key!.Value, x => x.Count, cancellationToken);
        return Result.Success<IReadOnlyList<ProductionQuoteSourceResponse>>(sent.Select(x =>
            new ProductionQuoteSourceResponse(x.Id, x.Number, x.ClientName, x.ResponsibleName, x.LastSentAt,
                counts.GetValueOrDefault(x.Id))).ToArray());
    }
}
