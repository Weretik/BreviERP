using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;

/// <summary>«Вишивка» of the month: the stitches and who embroidered them.</summary>
public sealed record SetPayrollEmbroideryCommand(ProductionActor Actor, string Month, SetPayrollEmbroideryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
