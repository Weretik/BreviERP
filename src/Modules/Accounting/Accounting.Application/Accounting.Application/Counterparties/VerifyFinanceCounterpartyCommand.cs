using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;

namespace Accounting.Application.Counterparties;

/// <summary>
/// «Підтвердити» a finance counterparty. Its active operations leave «Потребують перевірки» (those without a category
/// stay there for the category) without a new version: the operations themselves did not change.
/// </summary>
public sealed record VerifyFinanceCounterpartyCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class VerifyFinanceCounterpartyCommandValidator : AbstractValidator<VerifyFinanceCounterpartyCommand>
{
    public VerifyFinanceCounterpartyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class VerifyFinanceCounterpartyCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<VerifyFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(VerifyFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            async counterparty =>
            {
                if (!counterparty.Verify(now)) return null;
                foreach (var operation in await FinanceCounterpartyChange.OperationsOfAsync(db, counterparty, cancellationToken))
                    operation.FollowCounterparty(counterparty.State);
                return AuditAction.Verify;
            },
            cancellationToken);
    }
}
