using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.UpdateFinanceTransaction;

/// <summary>«Змінити» an active hand-made operation; the area stays, a stale <paramref name="Version"/> gets 409.</summary>
public sealed record UpdateFinanceTransactionCommand(Guid Id, FinanceTransactionFields Fields, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;
