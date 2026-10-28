using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteVersion;

public sealed record GetQuoteVersionQuery(Guid QuoteId, Guid VersionId, CrmActor Actor) : IQuery<Result<QuoteVersionView>>;
