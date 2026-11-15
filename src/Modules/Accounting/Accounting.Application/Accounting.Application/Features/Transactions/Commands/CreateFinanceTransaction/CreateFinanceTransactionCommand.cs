using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;

/// <summary>«Додати дохід» / «Додати витрату» in the current books or in «Домашня бухгалтерія» (<paramref name="Scope"/>).</summary>
/// <remarks><paramref name="RequestId"/> is made when the form opens: a repeated «Зберегти» returns the saved one.</remarks>
public sealed record CreateFinanceTransactionCommand(LedgerScope Scope, FinanceTransactionFields Fields, string By,
    Guid? RequestId = null)
    : ICommand<Result<FinanceTransactionResponse>>;
