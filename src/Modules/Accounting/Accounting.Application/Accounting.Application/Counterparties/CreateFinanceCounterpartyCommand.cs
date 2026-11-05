using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;

namespace Accounting.Application.Counterparties;

/// <summary>
/// «Додати фінансового» (<paramref name="Verified"/> false: it waits for «Підтвердити») or «Зберегти як фінансового
/// контрагента» from an operation (true: the director has just checked it).
/// </summary>
public sealed record CreateFinanceCounterpartyCommand(FinanceCounterpartyFields Fields, bool Verified, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class CreateFinanceCounterpartyCommandValidator : AbstractValidator<CreateFinanceCounterpartyCommand>
{
    public CreateFinanceCounterpartyCommandValidator()
    {
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceCounterpartyFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class CreateFinanceCounterpartyCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(CreateFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var details = command.Fields.ToDetails();
        var (category, missing) = await command.Fields.LoadCategoryAsync(db, cancellationToken);
        if (missing) return Result.NotFound(AccountingMessages.DefaultCategoryNotFound);

        var now = clock.GetUtcNow();
        var counterparty = FinanceCounterparty.Create(FinanceCounterpartyId.New(), details, category, command.Verified, now);
        db.Counterparties.Add(counterparty);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        AuditTrail.Add(db, AuditEntity.Counterparty, counterparty.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(counterparty, names));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceCounterpartyResponse.From(counterparty, names, 0));
    }
}
