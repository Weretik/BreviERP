using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.PrepareQuote;

public sealed record PrepareQuoteCommand(Guid QuoteId, Guid VersionId, long ExpectedRevision,
    Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
