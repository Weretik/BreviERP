using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollAdjustment;

public sealed class VoidPayrollAdjustmentValidator : AbstractValidator<VoidPayrollAdjustmentCommand>
{
    public VoidPayrollAdjustmentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.AdjustmentId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).Must(PayrollInputRules.IsReason).WithMessage(PayrollMessages.Reason)
            .When(x => x.Input is not null);
    }
}
