using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Queries.GetWorkshopStaff;

public sealed class GetWorkshopStaffHandler(IProductionDbContext db)
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
