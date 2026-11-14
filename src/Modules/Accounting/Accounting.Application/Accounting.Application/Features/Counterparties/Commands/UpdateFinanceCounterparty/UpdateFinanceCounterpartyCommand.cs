using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;

/// <summary>«Змінити» a finance counterparty; a stale <paramref name="Version"/> gets 409. Operations keep their link.</summary>
public sealed record UpdateFinanceCounterpartyCommand(Guid Id, FinanceCounterpartyFields Fields, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;
