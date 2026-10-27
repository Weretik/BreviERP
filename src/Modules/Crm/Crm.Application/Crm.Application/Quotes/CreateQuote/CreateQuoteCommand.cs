using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.CreateQuote;

public sealed record CreateQuoteCommand(
    Guid? CustomerPublicId, Guid? ContactPublicId, string? RecipientName,
    QuoteDraftContent Content, Guid RequestId, CrmActor Actor)
    : ICommand<Result<QuoteDetail>>;
