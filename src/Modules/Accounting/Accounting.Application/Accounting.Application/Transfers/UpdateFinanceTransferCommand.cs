using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Transfers;

/// <summary>«Змінити» an active hand-made transfer; the area stays, a stale <paramref name="Version"/> gets 409.</summary>
public sealed record UpdateFinanceTransferCommand(Guid Id, FinanceTransferFields Fields, int Version, string By)
    : ICommand<Result<FinanceTransferResponse>>;

public sealed class UpdateFinanceTransferCommandValidator : AbstractValidator<UpdateFinanceTransferCommand>
{
    public UpdateFinanceTransferCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransferFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class UpdateFinanceTransferCommandHandler(IAccountingDbContext db, TimeProvider clock)
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
