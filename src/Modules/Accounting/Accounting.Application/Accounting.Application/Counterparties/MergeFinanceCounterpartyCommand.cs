using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;

namespace Accounting.Application.Counterparties;

/// <summary>
/// «Об'єднати»: <paramref name="Id"/> is a duplicate of <paramref name="TargetId"/>. Its operations move to the target,
/// its names become other spellings of the target, and it is archived. Both versions are the ones the user saw.
/// </summary>
public sealed record MergeFinanceCounterpartyCommand(Guid Id, int Version, Guid TargetId, int TargetVersion, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class MergeFinanceCounterpartyCommandValidator : AbstractValidator<MergeFinanceCounterpartyCommand>
{
    public MergeFinanceCounterpartyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TargetId).NotEmpty().NotEqual(x => x.Id).WithMessage("Оберіть іншого контрагента.");
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.TargetVersion).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class MergeFinanceCounterpartyCommandHandler(IAccountingDbContext db, TimeProvider clock)
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
