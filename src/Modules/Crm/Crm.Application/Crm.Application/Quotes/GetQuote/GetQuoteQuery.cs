using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.GetQuote;

public sealed record GetQuoteQuery(QuoteActor Actor, long Id) : IQuery<Result<QuoteResponse>>;
