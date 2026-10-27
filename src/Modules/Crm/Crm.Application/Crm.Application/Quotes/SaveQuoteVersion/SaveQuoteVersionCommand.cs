using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.SaveQuoteVersion;

public sealed record SaveQuoteVersionCommand(
    Guid QuoteId, long ExpectedRevision, QuoteDraftContent Content,
    Guid RequestId, CrmActor Actor) : ICommand<Result<QuoteDetail>>;
