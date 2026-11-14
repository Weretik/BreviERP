using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Commands.DeleteFinanceAccount;

/// <summary>
/// «Видалити» an account. The log keeps its last snapshot. An account with any movement (cancelled ones too) can only
/// be archived: 409.
/// </summary>
public sealed record DeleteFinanceAccountCommand(Guid Id, int Version, string By) : ICommand<Result>;
