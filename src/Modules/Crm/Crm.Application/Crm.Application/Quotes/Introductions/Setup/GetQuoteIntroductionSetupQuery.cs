using Crm.Application.Quotes.Introductions.Contracts;

namespace Crm.Application.Quotes.Introductions.Setup;

/// <summary>The fixed main models of the pages and the letter variants (R-16, R-17).</summary>
public sealed record GetQuoteIntroductionSetupQuery : IQuery<Result<QuoteIntroductionSetupResponse>>;
