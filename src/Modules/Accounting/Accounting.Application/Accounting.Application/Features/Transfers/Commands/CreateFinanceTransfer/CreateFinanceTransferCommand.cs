using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.CreateFinanceTransfer;

/// <summary>«Додати переказ» in the current books or in «Домашня бухгалтерія» (<paramref name="Scope"/>).</summary>
public sealed record CreateFinanceTransferCommand(LedgerScope Scope, FinanceTransferFields Fields, string By)
    : ICommand<Result<FinanceTransferResponse>>;
