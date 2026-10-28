using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.CopyQuote;

public sealed record CopyQuoteCommand(Guid QuoteId, Guid VersionId, long ExpectedRevision,
    Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
