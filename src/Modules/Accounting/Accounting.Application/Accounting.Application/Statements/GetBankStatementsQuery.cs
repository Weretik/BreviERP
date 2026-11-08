using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;

namespace Accounting.Application.Statements;

/// <summary>«Історія виписок»: the last 100 uploaded statements of all accounts, newest first.</summary>
public sealed record GetBankStatementsQuery : IQuery<Result<IReadOnlyList<BankStatementResponse>>>;

public sealed class GetBankStatementsQueryHandler(IAccountingDbContext db)
    : IQueryHandler<GetBankStatementsQuery, Result<IReadOnlyList<BankStatementResponse>>>
{
    public const int Limit = 100;

    public async ValueTask<Result<IReadOnlyList<BankStatementResponse>>> Handle(GetBankStatementsQuery query,
        CancellationToken cancellationToken)
    {
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var rows = await db.BankStatements.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(Limit)
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<BankStatementResponse>>(
            rows.Select(x => BankStatementResponse.From(x, names.Account(x.AccountId))).ToList());
    }
}
