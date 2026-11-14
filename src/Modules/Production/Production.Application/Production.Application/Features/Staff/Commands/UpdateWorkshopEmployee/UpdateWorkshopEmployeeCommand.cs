using Production.Application.Abstractions;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.UpdateWorkshopEmployee;

public sealed record UpdateWorkshopEmployeeCommand(int Id, WorkshopEmployeeInput Input, int? UserId)
    : ICommand<Result<WorkshopEmployeeResponse>>;
