using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;

/// <summary>
/// «Додати фінансового» (<paramref name="Verified"/> false: it waits for «Підтвердити») or «Зберегти як фінансового
/// контрагента» from an operation (true: the director has just checked it).
/// </summary>
public sealed record CreateFinanceCounterpartyCommand(FinanceCounterpartyFields Fields, bool Verified, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class CreateFinanceCounterpartyValidator : AbstractValidator<CreateFinanceCounterpartyCommand>
{
    public CreateFinanceCounterpartyValidator()
    {
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceCounterpartyFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class CreateFinanceCounterpartyHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(CreateFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var details = command.Fields.ToDetails();
        if (details.IsFailure) return details.Error!.ToResult<FinanceCounterpartyResponse>();
        var (category, missing) = await command.Fields.LoadCategoryAsync(db, cancellationToken);
        if (missing) return Result.NotFound(AccountingMessages.DefaultCategoryNotFound);

        var now = clock.GetUtcNow();
        var created = FinanceCounterparty.Create(FinanceCounterpartyId.New(), details.Value, category, command.Verified, now);
        if (created.IsFailure) return created.Error!.ToResult<FinanceCounterpartyResponse>();
        var counterparty = created.Value;
        db.Counterparties.Add(counterparty);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        AuditTrail.Add(db, AuditEntity.Counterparty, counterparty.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(counterparty, names));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceCounterpartyResponse.From(counterparty, names));
    }
}
