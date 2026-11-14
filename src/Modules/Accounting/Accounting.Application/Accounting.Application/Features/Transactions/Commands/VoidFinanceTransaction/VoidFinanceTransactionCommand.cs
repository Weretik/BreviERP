using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;

/// <summary>
/// «Анулювати»: the operation is never deleted; it stays in the journal («Показати анульовані») and in «Історія» but no
/// longer counts in balances and totals. There is no way back for a hand-made operation.
/// </summary>
public sealed record VoidFinanceTransactionCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;
