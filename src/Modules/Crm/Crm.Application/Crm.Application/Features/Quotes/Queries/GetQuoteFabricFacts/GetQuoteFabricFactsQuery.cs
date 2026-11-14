using Crm.Application.Features.Quotes.Shared;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;

/// <summary>The manager line of each fabric in the КП editor, the same for a new, saved or changed variant (R-31).</summary>
public sealed record GetQuoteFabricFactsQuery(IReadOnlyList<int> FabricIds)
    : IQuery<Result<IReadOnlyList<QuoteFabricFactsResponse>>>;
