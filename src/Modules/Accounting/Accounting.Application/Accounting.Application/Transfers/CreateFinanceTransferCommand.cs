using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Transfers;

/// <summary>«Додати переказ» in the current books or in «Домашня бухгалтерія» (<paramref name="Scope"/>).</summary>
public sealed record CreateFinanceTransferCommand(LedgerScope Scope, FinanceTransferFields Fields, string By)
    : ICommand<Result<FinanceTransferResponse>>;

public sealed class CreateFinanceTransferCommandValidator : AbstractValidator<CreateFinanceTransferCommand>
{
    public CreateFinanceTransferCommandValidator()
    {
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransferFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class CreateFinanceTransferCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceTransferCommand, Result<FinanceTransferResponse>>
{
    public async ValueTask<Result<FinanceTransferResponse>> Handle(CreateFinanceTransferCommand command,
        CancellationToken cancellationToken)
    {
        var fields = command.Fields.ToDetails();
        if (fields.IsFailure) return fields.Error!.ToResult<FinanceTransferResponse>();
        var details = fields.Value;
        var (from, to, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);

        var now = clock.GetUtcNow();
        var created = FinanceTransfer.Create(FinanceTransferId.New(), command.Scope, details, from, to, command.By, now);
        if (created.IsFailure) return created.Error!.ToResult<FinanceTransferResponse>();
        var transfer = created.Value;
        db.Transfers.Add(transfer);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        AuditTrail.Add(db, AuditEntity.Transfer, transfer.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(transfer, names));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceTransferResponse.From(transfer, names));
    }
}
