using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.SetEmployeeActive;

/// <summary>«В архів» / «Повернути»: an archived employee disappears from pickers but keeps their history.</summary>
public sealed record SetWorkshopEmployeeActiveCommand(int Id, bool IsActive, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
