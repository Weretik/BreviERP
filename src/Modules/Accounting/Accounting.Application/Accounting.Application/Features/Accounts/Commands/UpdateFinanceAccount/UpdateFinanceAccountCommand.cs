using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Commands.UpdateFinanceAccount;

/// <summary>
/// «Змінити рахунок»: <paramref name="Version"/> is the version the user opened; a stale one is refused. With movements
/// the currency is fixed and the opening date stays on or before the earliest movement.
/// </summary>
public sealed record UpdateFinanceAccountCommand(Guid Id, FinanceAccountFields Fields, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;
