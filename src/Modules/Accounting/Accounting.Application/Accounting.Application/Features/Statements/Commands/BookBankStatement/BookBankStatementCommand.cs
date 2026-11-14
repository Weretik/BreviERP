using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace Accounting.Application.Features.Statements.Commands.BookBankStatement;

/// <summary>
/// «Провести виписку»: every included line that can be booked becomes an operation or a transfer in one save, after
/// the duplicates are checked again. A line with an unknown counterparty creates an unconfirmed finance counterparty
/// (one per ЄДРПОУ, IBAN or name in the statement). A stale <paramref name="Version"/> gets 409.
/// </summary>
public sealed record BookBankStatementCommand(Guid Id, int Version, string By) : ICommand<Result<BankStatementResponse>>;
