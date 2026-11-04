namespace Crm.Application.Quotes.CustomSewing;

public sealed class GetQuoteModelCompositionQueryHandler(IQuoteModelCompositionReader reader)
    : IQueryHandler<GetQuoteModelCompositionQuery, Result<QuoteModelCompositionResponse>>
{
    public const string NotFound = "Модель не знайдено в каталозі.";

    public async ValueTask<Result<QuoteModelCompositionResponse>> Handle(
        GetQuoteModelCompositionQuery query, CancellationToken cancellationToken)
    {
        var composition = await reader.ReadAsync(query.ProductId, cancellationToken);
        if (composition is null) return Result.NotFound(NotFound);
        return Result.Success(new QuoteModelCompositionResponse(composition.ProductId,
            composition.Fabrics.Select(x => new QuoteModelFabricResponse(x.ArticleId, x.FabricId, x.Name, x.Meters,
                x.IsActive)).ToArray(),
            composition.Lines.Select(x => new QuoteModelLineResponse(x.Kind.ToString(), x.ReferenceId, x.CostCode,
                x.Name, x.Unit, x.Quantity, x.ManualMinutes)).ToArray()));
    }
}
