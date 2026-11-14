using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Products;
namespace Crm.Application.Features.Quotes.Queries.CalculateQuoteCustomSewing;

public sealed class CalculateQuoteCustomSewingHandler(IQuoteSewingCalculator calculator)
    : IQueryHandler<CalculateQuoteCustomSewingQuery, Result<QuoteCustomSewingPriceResponse>>
{
    public async ValueTask<Result<QuoteCustomSewingPriceResponse>> Handle(
        CalculateQuoteCustomSewingQuery query, CancellationToken cancellationToken)
    {
        var composition = new QuoteSewingComposition(
            query.Input.Variants.Select(x => x.Fabrics.ToVariant()).ToArray(),
            query.Input.Lines.Select(x => x.ToLine()).ToArray());
        var calculation = await calculator.CalculateAsync(composition, cancellationToken);
        return Result.Success(calculation.ToResponse());
    }
}
