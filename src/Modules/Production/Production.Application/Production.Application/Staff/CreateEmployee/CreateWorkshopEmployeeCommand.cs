using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Staff;

namespace Production.Application.Staff.CreateEmployee;

public sealed record CreateWorkshopEmployeeCommand(WorkshopEmployeeInput Input, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
