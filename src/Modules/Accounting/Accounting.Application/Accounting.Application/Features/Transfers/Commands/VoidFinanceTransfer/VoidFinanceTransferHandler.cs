using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.VoidFinanceTransfer;

public sealed class VoidFinanceTransferHandler(IAccountingDbContext db, TimeProvider clock)
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
        var voided = transfer.Void(now);
        if (voided.IsFailure) return voided.Error!.ToResult<FinanceTransferResponse>();
        AuditTrail.Add(db, AuditEntity.Transfer, command.Id, AuditAction.Void, command.By, now, before,
            AuditTrail.Snapshot(transfer, names));
        return await FinanceTransferSave.SaveAsync(db, transfer, names, cancellationToken);
    }
}
