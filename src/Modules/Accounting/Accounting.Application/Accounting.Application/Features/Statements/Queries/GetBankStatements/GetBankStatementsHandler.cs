using Accounting.Application.Abstractions;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatements;

public sealed class GetBankStatementsHandler(IAccountingDbContext db)
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
