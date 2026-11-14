using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;

public sealed class MergeFinanceCounterpartyHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<MergeFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    /// <returns>The target counterparty after the merge.</returns>
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(MergeFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var source = await FinanceCounterpartyChange.FindAsync(db, command.Id, cancellationToken);
        var target = await FinanceCounterpartyChange.FindAsync(db, command.TargetId, cancellationToken);
        if (source is null || target is null) return Result.NotFound(AccountingMessages.CounterpartyNotFound);
        if (source.Version != command.Version || target.Version != command.TargetVersion)
            return Result.Conflict(AccountingMessages.Stale);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var sourceBefore = AuditTrail.Snapshot(source, names);
        var targetBefore = AuditTrail.Snapshot(target, names);
        var now = clock.GetUtcNow();
        var operations = await FinanceCounterpartyChange.OperationsOfAsync(db, source, cancellationToken);
        var merged = source.MergeInto(target, now);
        if (merged.IsFailure) return merged.Error!.ToResult<FinanceCounterpartyResponse>();
        foreach (var operation in operations) operation.FollowCounterparty(target.State);

        AuditTrail.Add(db, AuditEntity.Counterparty, command.Id, AuditAction.Merge, command.By, now, sourceBefore,
            AuditTrail.Snapshot(source, names));
        AuditTrail.Add(db, AuditEntity.Counterparty, command.TargetId, AuditAction.Merge, command.By, now, targetBefore,
            AuditTrail.Snapshot(target, names));
        var saved = await FinanceCounterpartyChange.SaveAsync(db, cancellationToken);
        if (!saved.IsSuccess) return Result.Conflict(AccountingMessages.Stale);
        return Result.Success(await FinanceCounterpartyChange.ResponseAsync(db, target, names, cancellationToken));
    }
}
