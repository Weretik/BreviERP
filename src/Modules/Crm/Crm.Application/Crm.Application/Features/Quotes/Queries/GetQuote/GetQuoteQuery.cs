using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetQuote;

public sealed record GetQuoteQuery(QuoteActor Actor, long Id) : IQuery<Result<QuoteResponse>>;
