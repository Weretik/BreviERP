using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.DeleteQuote;

/// <summary>
/// «Видалити КП» (R-33): a КП of any status, with its items and history. Only who may delete (the Director for now);
/// a КП with a production order stays.
/// </summary>
public sealed record DeleteQuoteCommand(QuoteActor Actor, long Id, int Version) : ICommand<Result>;
