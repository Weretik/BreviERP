namespace Crm.Application.Quotes.CustomSewing;

public sealed class CalculateQuoteCustomSewingQueryHandler(IQuoteSewingCalculator calculator)
    : IQueryHandler<CalculateQuoteCustomSewingQuery, Result<QuoteCustomSewingPriceResponse>>
{
    public async ValueTask<Result<QuoteCustomSewingPriceResponse>> Handle(
        CalculateQuoteCustomSewingQuery query, CancellationToken cancellationToken)
    {
        var composition = new QuoteSewingComposition(
            query.Input.Fabrics.Select(x => new QuoteSewingFabric(x.FabricId, x.Meters)).ToArray(),
            query.Input.Lines.Select(x => x.ToLine()).ToArray());
        var calculation = await calculator.CalculateAsync(composition, cancellationToken);
        return Result.Success(calculation.ToResponse());
    }
}
