using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain;
using Production.Domain.Staff;

namespace Production.Application.Staff;

internal static class EmployeeLookup
{
    public static Task<bool> ActiveNameTakenAsync(IProductionDbContext db, string fullName, int? exceptId,
        CancellationToken cancellationToken)
    {
        var key = ProductionText.Key(fullName);
        return db.Employees.AnyAsync(x => x.IsActive && x.NameKey == key && x.Id != exceptId, cancellationToken);
    }

    public static WorkshopEmployeeResponse ToResponse(this WorkshopEmployee x) =>
        new(x.Id, x.FullName, x.Position, x.IsActive, x.UpdatedAt);
}
