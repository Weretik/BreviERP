using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;

namespace Accounting.Application.Features.History.Queries.GetFinanceHistory;

public sealed class GetFinanceHistoryHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceHistoryQuery, Result<IReadOnlyList<FinanceHistoryEntryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceHistoryEntryResponse>>> Handle(GetFinanceHistoryQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FinanceHistoryEntryResponse> rows = await db.AuditEntries.AsNoTracking()
            .Where(x => x.Entity == query.Entity && x.EntityId == query.Id)
            .OrderByDescending(x => x.Id)
            .Select(x => new FinanceHistoryEntryResponse(x.Id, x.Entity, x.EntityId, x.Action, x.By, x.At, x.BeforeJson,
                x.AfterJson))
            .ToListAsync(cancellationToken);
        return Result.Success(rows);
    }
}
