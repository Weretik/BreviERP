using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;

/// <summary>
/// «Об'єднати»: <paramref name="Id"/> is a duplicate of <paramref name="TargetId"/>. Its operations move to the target,
/// its names become other spellings of the target, and it is archived. Both versions are the ones the user saw.
/// </summary>
public sealed record MergeFinanceCounterpartyCommand(Guid Id, int Version, Guid TargetId, int TargetVersion, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;
