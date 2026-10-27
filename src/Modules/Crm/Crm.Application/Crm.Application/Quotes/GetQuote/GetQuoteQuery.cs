using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.GetQuote;

public sealed record GetQuoteQuery(Guid QuoteId, CrmActor Actor) : IQuery<Result<QuoteDetail>>;
