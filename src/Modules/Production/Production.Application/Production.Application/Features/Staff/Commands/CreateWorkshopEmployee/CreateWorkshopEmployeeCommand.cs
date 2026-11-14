using Production.Application.Abstractions;
using Production.Domain.Staff;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;

public sealed record CreateWorkshopEmployeeCommand(WorkshopEmployeeInput Input, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
