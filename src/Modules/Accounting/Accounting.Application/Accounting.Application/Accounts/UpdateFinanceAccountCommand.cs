using Accounting.Application.Abstractions;
using Accounting.Application.Balances;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>
/// «Змінити рахунок»: <paramref name="Version"/> is the version the user opened; a stale one is refused. With movements
/// the currency is fixed and the opening date stays on or before the earliest movement.
/// </summary>
public sealed record UpdateFinanceAccountCommand(Guid Id, FinanceAccountFields Fields, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;

public sealed class UpdateFinanceAccountCommandValidator : AbstractValidator<UpdateFinanceAccountCommand>
{
    public UpdateFinanceAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceAccountFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}

public sealed class UpdateFinanceAccountCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceAccountCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(UpdateFinanceAccountCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var details = command.Fields.ToDetails();
        if (details.IsFailure) return details.Error!.ToResult<FinanceAccountResponse>();
        var usage = await FinanceBalances.UsageAsync(db, FinanceAccountId.From(command.Id), cancellationToken);
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => account.Update(details.Value, usage, now).Map<AuditAction?>(changed => changed ? AuditAction.Update : null),
            cancellationToken);
    }
}
