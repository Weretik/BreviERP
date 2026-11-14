using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.ReviewFinanceTransaction;

/// <summary>
/// «Перевірити» an operation of «Потребують перевірки»: the category and who it is with, nothing else. A finance
/// counterparty must be confirmed first; a stale <paramref name="Version"/> gets 409. <paramref name="RememberCategory"/>
/// makes the category the default of the chosen finance counterparty and applies it to its next operations; it does
/// nothing for a client, a supplier or a one-off name.
/// </summary>
public sealed record ReviewFinanceTransactionCommand(Guid Id, Guid CategoryId, FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName, int Version, string By, bool RememberCategory = false)
    : ICommand<Result<FinanceTransactionResponse>>;
