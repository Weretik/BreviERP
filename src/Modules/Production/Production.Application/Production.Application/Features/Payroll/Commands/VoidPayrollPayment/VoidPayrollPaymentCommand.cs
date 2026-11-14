using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollPayment;

/// <summary>Cancels a payment with a reason; it stays in the list, crossed out.</summary>
public sealed record VoidPayrollPaymentCommand(ProductionActor Actor, int PaymentId, VoidPayrollEntryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
