using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.SetResponsible;

/// <summary>Director or Admin gives a КП to themselves or another manager (R-02).</summary>
public sealed record SetQuoteResponsibleCommand(QuoteActor Actor, long Id, int Version, int UserId)
    : ICommand<Result<QuoteResponse>>;
