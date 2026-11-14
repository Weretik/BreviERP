using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Commands.UpdateFinanceTransfer;

public sealed class UpdateFinanceTransferHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceTransferCommand, Result<FinanceTransferResponse>>
{
    public async ValueTask<Result<FinanceTransferResponse>> Handle(UpdateFinanceTransferCommand command,
        CancellationToken cancellationToken)
    {
        var fields = command.Fields.ToDetails();
        if (fields.IsFailure) return fields.Error!.ToResult<FinanceTransferResponse>();
        var details = fields.Value;
        var transfer = await db.Transfers.FirstOrDefaultAsync(x => x.Id == FinanceTransferId.From(command.Id),
            cancellationToken);
        if (transfer is null) return Result.NotFound(AccountingMessages.TransferNotFound);
        if (transfer.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        var (from, to, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var before = AuditTrail.Snapshot(transfer, names);
        var now = clock.GetUtcNow();
        var updated = transfer.Update(details, from, to, now);
        if (updated.IsFailure) return updated.Error!.ToResult<FinanceTransferResponse>();
        if (!updated.Value) return Result.Success(FinanceTransferResponse.From(transfer, names));

        AuditTrail.Add(db, AuditEntity.Transfer, command.Id, AuditAction.Update, command.By, now, before,
            AuditTrail.Snapshot(transfer, names));
        return await FinanceTransferSave.SaveAsync(db, transfer, names, cancellationToken);
    }
}
