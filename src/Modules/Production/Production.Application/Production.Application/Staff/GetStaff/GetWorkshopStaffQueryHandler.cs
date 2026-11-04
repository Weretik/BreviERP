using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.GetStaff;

public sealed class GetWorkshopStaffQueryHandler(IProductionDbContext db)
    : IQueryHandler<GetWorkshopStaffQuery, Result<IReadOnlyList<WorkshopEmployeeResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<WorkshopEmployeeResponse>>> Handle(GetWorkshopStaffQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await db.Employees.AsNoTracking()
            .Where(x => query.IncludeArchived || x.IsActive)
            .ToListAsync(cancellationToken);
        IReadOnlyList<WorkshopEmployeeResponse> result = rows
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.FullName, StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), true))
            .Select(x => x.ToResponse())
            .ToList();
        return Result.Success(result);
    }
}
