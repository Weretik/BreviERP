using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

/// <summary>«Виплата» of a month: an advance, a card or cash payment (PRD-D45).</summary>
public sealed record CreatePayrollPaymentCommand(ProductionActor Actor, string Month, CreatePayrollPaymentInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
