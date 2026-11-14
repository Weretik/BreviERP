using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;

/// <summary>
/// «Додати фінансового» (<paramref name="Verified"/> false: it waits for «Підтвердити») or «Зберегти як фінансового
/// контрагента» from an operation (true: the director has just checked it).
/// </summary>
public sealed record CreateFinanceCounterpartyCommand(FinanceCounterpartyFields Fields, bool Verified, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;
