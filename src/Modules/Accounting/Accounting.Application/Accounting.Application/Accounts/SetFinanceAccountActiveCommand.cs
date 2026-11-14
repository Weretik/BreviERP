using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>«Архівувати» (<paramref name="Active"/> false) or «Відновити» (true); history stays either way.</summary>
public sealed record SetFinanceAccountActiveCommand(Guid Id, bool Active, int Version, string By)
    : ICommand<Result<FinanceAccountResponse>>;

public sealed class SetFinanceAccountActiveCommandValidator : AbstractValidator<SetFinanceAccountActiveCommand>
{
    public SetFinanceAccountActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}

public sealed class SetFinanceAccountActiveCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetFinanceAccountActiveCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(SetFinanceAccountActiveCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => DomainResult<AuditAction?>.Success(account.SetActive(command.Active, now)
                ? command.Active ? AuditAction.Restore : AuditAction.Archive
                : null),
            cancellationToken);
    }
}
