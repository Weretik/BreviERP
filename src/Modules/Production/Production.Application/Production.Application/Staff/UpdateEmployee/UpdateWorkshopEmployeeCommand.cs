using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.UpdateEmployee;

public sealed record UpdateWorkshopEmployeeCommand(int Id, WorkshopEmployeeInput Input, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
