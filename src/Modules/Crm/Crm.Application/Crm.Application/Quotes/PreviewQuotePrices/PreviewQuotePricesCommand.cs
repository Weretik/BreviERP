using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Pricing;
using Mediator;

namespace Crm.Application.Quotes.PreviewQuotePrices;

public sealed record PreviewQuotePricesCommand(
    Guid RequestId, Guid? QuoteId, QuotePriceIntent Intent, CrmActor Actor)
    : ICommand<Result<QuotePricePreview>>;
