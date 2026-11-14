using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;

/// <summary>
/// «Підтвердити» a finance counterparty. Its active operations leave «Потребують перевірки» (those without a category
/// stay there for the category) without a new version: the operations themselves did not change.
/// </summary>
public sealed record VerifyFinanceCounterpartyCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;
