namespace Production.Application.Features.Work.Commands.CreateProductionWork;

/// <param name="Kind">Sewing or Cutting.</param>
/// <param name="Reason">One of the over-plan reasons; required when <paramref name="OverPlan"/> is set.</param>
public sealed record CreateProductionWorkInput(int EmployeeId, string Kind, int Quantity, DateOnly WorkDate,
    bool OverPlan, string? Reason, string? Comment);
