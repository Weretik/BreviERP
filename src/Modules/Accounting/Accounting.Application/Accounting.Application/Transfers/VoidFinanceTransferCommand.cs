using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Transfers;

/// <summary>«Анулювати»: the transfer stays in the journal and «Історія» but no longer moves money.</summary>
public sealed record VoidFinanceTransferCommand(Guid Id, int Version, string By) : ICommand<Result<FinanceTransferResponse>>;

public sealed class VoidFinanceTransferCommandValidator : AbstractValidator<VoidFinanceTransferCommand>
{
    public VoidFinanceTransferCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class VoidFinanceTransferCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<VoidFinanceTransferCommand, Result<FinanceTransferResponse>>
{
    public async ValueTask<Result<FinanceTransferResponse>> Handle(VoidFinanceTransferCommand command,
        CancellationToken cancellationToken)
    {
        var transfer = await db.Transfers.FirstOrDefaultAsync(x => x.Id == FinanceTransferId.From(command.Id),
            cancellationToken);
        if (transfer is null) return Result.NotFound(AccountingMessages.TransferNotFound);
        if (transfer.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var before = AuditTrail.Snapshot(transfer, names);
        var now = clock.GetUtcNow();
        transfer.Void(now);
        AuditTrail.Add(db, AuditEntity.Transfer, command.Id, AuditAction.Void, command.By, now, before,
            AuditTrail.Snapshot(transfer, names));
        return await FinanceTransferSave.SaveAsync(db, transfer, names, cancellationToken);
    }
}
