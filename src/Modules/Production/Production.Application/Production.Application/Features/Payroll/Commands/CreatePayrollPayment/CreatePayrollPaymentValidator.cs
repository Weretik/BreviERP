using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

public sealed class CreatePayrollPaymentValidator : AbstractValidator<CreatePayrollPaymentCommand>
{
    public CreatePayrollPaymentValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.EmployeeId).GreaterThan(0).WithMessage("Оберіть працівника.");
            RuleFor(x => x.Input.PaidOn).NotEmpty().WithMessage("Вкажіть дату виплати.");
            RuleFor(x => x.Input.Kind).Must(x => Enum.TryParse<PayrollPaymentKind>(x, out var k) && Enum.IsDefined(k))
                .WithMessage(PayrollMessages.PaymentKind);
            RuleFor(x => x.Input.Amount).Must(x => x is > 0 and <= PayrollPayment.MaxAmount && PayrollInputRules.IsKopecks(x))
                .WithMessage(PayrollMessages.Amount);
            RuleFor(x => x.Input.Comment).MaximumLength(PayrollPayment.CommentMaxLength);
        });
    }
}
