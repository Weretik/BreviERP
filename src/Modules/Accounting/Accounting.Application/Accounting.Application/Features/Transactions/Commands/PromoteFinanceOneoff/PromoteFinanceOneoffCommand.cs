using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.PromoteFinanceOneoff;

/// <summary>
/// «Зробити постійним» on a one-off name: a checked finance counterparty is created from it and the operation is linked
/// to it. Other operations with the same name stay as they are (names are never joined by themselves).
/// </summary>
public sealed record PromoteFinanceOneoffCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;
