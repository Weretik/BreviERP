using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>
/// «Залишок на 01.10»: confirms the starting balance of the current books (who and when are stored) or clears it, so
/// the account shows «Не підтверджено» again.
/// </summary>
public sealed record SetCutoverBalanceCommand(Guid Id, long? BalanceMinor, bool Confirmed, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;

public sealed class SetCutoverBalanceCommandValidator : AbstractValidator<SetCutoverBalanceCommand>
{
    public SetCutoverBalanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BalanceMinor).NotNull().When(x => x.Confirmed).WithMessage("Вкажіть залишок на 01.10.2026.");
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}

public sealed class SetCutoverBalanceCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetCutoverBalanceCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(SetCutoverBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => account.SetCutoverBalance(command.BalanceMinor, command.Confirmed, command.By, now)
                ? command.Confirmed ? AuditAction.CutoverConfirm : AuditAction.CutoverReset
                : null,
            cancellationToken);
    }
}
