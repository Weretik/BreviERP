using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.SetPayrollIssued;

public sealed class SetPayrollIssuedValidator : AbstractValidator<SetPayrollIssuedCommand>
{
    public SetPayrollIssuedValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.EmployeeId).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Amount)
            .Must(x => x is >= 0 and <= PayrollPayment.MaxAmount && PayrollInputRules.IsKopecks(x.Value))
            .WithMessage("Сума від 0 до 10 000 000 грн, з точністю до копійки.")
            .When(x => x.Input?.Amount is not null);
    }
}
