using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;

public sealed class CreatePayrollAdjustmentValidator : AbstractValidator<CreatePayrollAdjustmentCommand>
{
    public CreatePayrollAdjustmentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.EmployeeId).GreaterThan(0).WithMessage("Оберіть працівника.");
            RuleFor(x => x.Input.Amount)
                .Must(x => x != 0 && Math.Abs(x) <= PayrollPayment.MaxAmount && PayrollInputRules.IsKopecks(x))
                .WithMessage(PayrollMessages.AdjustmentAmount);
            RuleFor(x => x.Input.Reason).Must(PayrollInputRules.IsReason).WithMessage(PayrollMessages.Reason);
        });
    }
}
