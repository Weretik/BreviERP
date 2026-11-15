using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;
using BuildingBlocks.Application;

namespace Accounting.Application.Features.Transfers.Commands.CreateFinanceTransfer;

public sealed class CreateFinanceTransferHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceTransferCommand, Result<FinanceTransferResponse>>
{
    public async ValueTask<Result<FinanceTransferResponse>> Handle(CreateFinanceTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (await RepeatAsync(command.RequestId, cancellationToken) is { } repeat) return repeat;
        var fields = command.Fields.ToDetails();
        if (fields.IsFailure) return fields.Error!.ToResult<FinanceTransferResponse>();
        var details = fields.Value;
        var (from, to, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);

        var now = clock.GetUtcNow();
        var created = FinanceTransfer.Create(FinanceTransferId.New(), command.Scope, details, from, to, command.By, now,
            command.RequestId);
        if (created.IsFailure) return created.Error!.ToResult<FinanceTransferResponse>();
        var transfer = created.Value;
        db.Transfers.Add(transfer);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        AuditTrail.Add(db, AuditEntity.Transfer, transfer.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(transfer, names));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (command.RequestId is not null)
        {
            // The same request saved meanwhile (a double click): answer with the saved transfer.
            if (await RepeatAsync(command.RequestId, cancellationToken) is { } saved) return saved;
            throw;
        }

        return Result.Success(FinanceTransferResponse.From(transfer, names));
    }

    private async Task<Result<FinanceTransferResponse>?> RepeatAsync(Guid? requestId,
        CancellationToken cancellationToken)
    {
        if (requestId is not { } id) return null;
        var saved = await db.Transfers.AsNoTracking().SingleOrDefaultAsync(x => x.RequestId == id, cancellationToken);
        return saved is null
            ? null
            : Result.Success(FinanceTransferResponse.From(saved, await FinanceNames.LoadAsync(db, cancellationToken)));
    }
}
