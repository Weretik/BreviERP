using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.RepriceQuote;

public sealed record RepriceQuoteCommand(Guid QuoteId, Guid VersionId, long ExpectedRevision,
    IReadOnlyList<QuoteRepriceBasis> Bases, Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
