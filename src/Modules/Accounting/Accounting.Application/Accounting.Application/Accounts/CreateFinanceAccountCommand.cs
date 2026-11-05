using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>«Додати рахунок».</summary>
public sealed record CreateFinanceAccountCommand(FinanceAccountFields Fields, string By)
    : ICommand<Result<FinanceAccountResponse>>;

public sealed class CreateFinanceAccountCommandValidator : AbstractValidator<CreateFinanceAccountCommand>
{
    public CreateFinanceAccountCommandValidator()
    {
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceAccountFieldsValidator());
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}

public sealed class CreateFinanceAccountCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceAccountCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(CreateFinanceAccountCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var account = FinanceAccount.Create(FinanceAccountId.New(), command.Fields.ToDetails(), now);
        db.Accounts.Add(account);
        AuditTrail.Add(db, AuditEntity.Account, account.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(account));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceAccountResponse.From(account));
    }
}
