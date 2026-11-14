using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollPayment;

public sealed class VoidPayrollPaymentValidator : AbstractValidator<VoidPayrollPaymentCommand>
{
    public VoidPayrollPaymentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.PaymentId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).Must(PayrollInputRules.IsReason).WithMessage(PayrollMessages.Reason)
            .When(x => x.Input is not null);
    }
}
