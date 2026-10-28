using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.TransferQuote;

public sealed record TransferQuoteCommand(Guid QuoteId, long ExpectedRevision, int OwnerId,
    string Reason, Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
