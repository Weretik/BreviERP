using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.Archive;

/// <summary>«В архів» or «Повернути з архіву» (R-23); any status, by whoever may open the КП.</summary>
public sealed record SetQuoteArchivedCommand(QuoteActor Actor, long Id, int Version, bool Archived)
    : ICommand<Result<QuoteResponse>>;
