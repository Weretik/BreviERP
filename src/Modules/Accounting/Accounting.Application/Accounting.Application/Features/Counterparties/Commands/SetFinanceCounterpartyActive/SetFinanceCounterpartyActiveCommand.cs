using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;

/// <summary>
/// «Архівувати» (<paramref name="Active"/> false) or «Відновити» (true). An archived counterparty cannot be chosen for
/// new operations; the ones that already have it keep the link.
/// </summary>
public sealed record SetFinanceCounterpartyActiveCommand(Guid Id, bool Active, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;
