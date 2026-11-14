using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.SetQuoteResponsible;

/// <summary>Director or Admin gives a КП to themselves or another manager (R-02).</summary>
public sealed record SetQuoteResponsibleCommand(QuoteActor Actor, long Id, int Version, int UserId)
    : ICommand<Result<QuoteResponse>>;
