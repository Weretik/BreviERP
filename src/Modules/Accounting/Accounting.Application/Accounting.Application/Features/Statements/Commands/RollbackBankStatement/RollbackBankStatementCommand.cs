using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;

namespace Accounting.Application.Features.Statements.Commands.RollbackBankStatement;

/// <summary>
/// «Відкотити виписку»: cancels every active operation and transfer the statement booked (they stay in the journal as
/// cancelled and give up their bank identity, so the lines are no duplicates when booked again), archives the unconfirmed finance counterparties the booking created when nothing active uses them, and
/// turns the statement into a preview that can be corrected and booked again. One save; a stale
/// <paramref name="Version"/> gets 409.
/// </summary>
public sealed record RollbackBankStatementCommand(Guid Id, int Version, string By) : ICommand<Result<BankStatementResponse>>;
