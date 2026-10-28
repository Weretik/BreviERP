using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.ArchiveQuote;

public sealed record ArchiveQuoteCommand(Guid QuoteId, long ExpectedRevision, bool Archived,
    string Reason, Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
