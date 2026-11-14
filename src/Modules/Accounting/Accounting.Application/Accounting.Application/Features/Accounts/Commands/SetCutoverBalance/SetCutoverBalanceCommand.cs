using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;

/// <summary>
/// «Залишок на 01.10»: confirms the starting balance of the current books (who and when are stored) or clears it, so
/// the account shows «Не підтверджено» again.
/// </summary>
public sealed record SetCutoverBalanceCommand(Guid Id, long? BalanceMinor, bool Confirmed, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;
