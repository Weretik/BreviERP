using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteIntroductionSetup;

/// <summary>The fixed main models of the pages and the letter variants (R-16, R-17).</summary>
public sealed record GetQuoteIntroductionSetupQuery : IQuery<Result<QuoteIntroductionSetupResponse>>;
