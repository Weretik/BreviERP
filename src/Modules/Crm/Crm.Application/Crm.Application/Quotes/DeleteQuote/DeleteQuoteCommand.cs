using Crm.Application.Quotes.Access;

namespace Crm.Application.Quotes.DeleteQuote;

/// <summary>Deletes a draft; a sent КП stays as the record of what the client got (R-13).</summary>
public sealed record DeleteQuoteCommand(QuoteActor Actor, long Id, int Version) : ICommand<Result>;
