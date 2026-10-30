using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Quotes.Introductions;
using Mediator;
using Crm.Application.Quotes.Introductions;

namespace Crm.Application.Quotes.CreateQuote;

public sealed record CreateQuoteCommand(
    Guid? CustomerPublicId, Guid? ContactPublicId, string? RecipientName,
    QuoteDraftContent Content, Guid RequestId, CrmActor Actor,
    IntroductionPublicSnapshot? IntroductionPublicSnapshot = null,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] IntroductionQuoteApplicationIntent? IntroductionApplicationIntent = null)
    : ICommand<Result<QuoteDetail>>;
