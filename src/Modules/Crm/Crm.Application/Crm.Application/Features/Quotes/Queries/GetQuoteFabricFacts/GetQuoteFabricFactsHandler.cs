using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;

public sealed class GetQuoteFabricFactsHandler(IQuoteFabricFactsReader reader)
    : IQueryHandler<GetQuoteFabricFactsQuery, Result<IReadOnlyList<QuoteFabricFactsResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<QuoteFabricFactsResponse>>> Handle(
        GetQuoteFabricFactsQuery query, CancellationToken cancellationToken)
    {
        var ids = query.FabricIds.Distinct().ToArray();
        var facts = await reader.GetAsync(ids, cancellationToken);
        return Result.Success<IReadOnlyList<QuoteFabricFactsResponse>>(facts
            .Where(x => ids.Contains(x.FabricId))
            .Select(x => new QuoteFabricFactsResponse(x.FabricId, x.Name, Clean(x.Composition), Clean(x.Density),
                x.PricePerMeter, x.SupplierId, Clean(x.SupplierName)))
            .ToArray());
    }

    private static string? Clean(string? text) =>
        string.IsNullOrWhiteSpace(text) || text.Trim() == "-" ? null : text.Trim();
}
