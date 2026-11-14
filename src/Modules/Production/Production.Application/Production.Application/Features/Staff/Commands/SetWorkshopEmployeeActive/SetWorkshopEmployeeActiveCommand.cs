using Production.Application.Abstractions;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.SetWorkshopEmployeeActive;

/// <summary>«В архів» / «Повернути»: an archived employee disappears from pickers but keeps their history.</summary>
public sealed record SetWorkshopEmployeeActiveCommand(int Id, bool IsActive, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
