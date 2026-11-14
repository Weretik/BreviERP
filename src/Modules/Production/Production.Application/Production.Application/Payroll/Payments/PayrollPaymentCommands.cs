using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Application.Work;
using Production.Domain.Payroll;

namespace Production.Application.Payroll.Payments;

internal static class PayrollAmounts
{
    public static bool IsKopecks(decimal value) => decimal.Round(value, 2) == value;

    public static bool IsReason(string? value) =>
        value?.Trim() is { Length: >= 3 and <= PayrollPayment.ReasonMaxLength };
}

/// <summary>«Виплата» of a month: an advance, a card or cash payment (PRD-D45).</summary>
public sealed record CreatePayrollPaymentCommand(ProductionActor Actor, string Month, CreatePayrollPaymentInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class CreatePayrollPaymentCommandValidator : AbstractValidator<CreatePayrollPaymentCommand>
{
    public CreatePayrollPaymentCommandValidator()
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
            RuleFor(x => x.Input.Amount).Must(x => x is > 0 and <= PayrollPayment.MaxAmount && PayrollAmounts.IsKopecks(x))
                .WithMessage(PayrollMessages.Amount);
            RuleFor(x => x.Input.Comment).MaximumLength(PayrollPayment.CommentMaxLength);
        });
    }
}

public sealed class CreatePayrollPaymentCommandHandler(IProductionDbContext db, PayrollChanges changes,
    TimeProvider clock) : ICommandHandler<CreatePayrollPaymentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(CreatePayrollPaymentCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var input = command.Input;
            if (input.PaidOn > WorkshopDay.Today(clock))
                return Result.Invalid(new ValidationError("Input.PaidOn", PayrollMessages.FuturePayment));
            var employee = await db.Employees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.EmployeeId, cancellationToken);
            if (employee is null) return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var payment = PayrollPayment.Create(
                new PayrollEntryTarget(change.Period.Month, employee.Id, employee.FullName), input.PaidOn,
                Enum.Parse<PayrollPaymentKind>(input.Kind), input.Amount, input.Comment, change.ActorName, change.Now);
            if (payment.IsFailure) return payment.Error!.ToResult();
            db.PayrollPayments.Add(payment.Value);
            return null;
        }, cancellationToken);
}

/// <summary>Cancels a payment with a reason; it stays in the list, crossed out.</summary>
public sealed record VoidPayrollPaymentCommand(ProductionActor Actor, int PaymentId, VoidPayrollEntryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class VoidPayrollPaymentCommandValidator : AbstractValidator<VoidPayrollPaymentCommand>
{
    public VoidPayrollPaymentCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.PaymentId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).Must(PayrollAmounts.IsReason).WithMessage(PayrollMessages.Reason)
            .When(x => x.Input is not null);
    }
}

public sealed class VoidPayrollPaymentCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<VoidPayrollPaymentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(VoidPayrollPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await db.PayrollPayments.SingleOrDefaultAsync(x => x.Id == command.PaymentId, cancellationToken);
        if (payment is null) return Result<PayrollMonthResponse>.NotFound(PayrollMessages.PaymentNotFound);
        return await changes.ApplyAsync(command.Actor, PayrollMonths.Format(payment.Month), adminOnly: true, change =>
            Task.FromResult(payment.Void(command.Input.Reason, change.ActorName, change.Now).FailureOrNull()),
            cancellationToken);
    }
}

/// <summary>«Інше нарахування / утримання» of a month with a reason (PRD-D45).</summary>
public sealed record CreatePayrollAdjustmentCommand(ProductionActor Actor, string Month,
    CreatePayrollAdjustmentInput Input) : ICommand<Result<PayrollMonthResponse>>;

public sealed class CreatePayrollAdjustmentCommandValidator : AbstractValidator<CreatePayrollAdjustmentCommand>
{
    public CreatePayrollAdjustmentCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.EmployeeId).GreaterThan(0).WithMessage("Оберіть працівника.");
            RuleFor(x => x.Input.Amount)
                .Must(x => x != 0 && Math.Abs(x) <= PayrollPayment.MaxAmount && PayrollAmounts.IsKopecks(x))
                .WithMessage(PayrollMessages.AdjustmentAmount);
            RuleFor(x => x.Input.Reason).Must(PayrollAmounts.IsReason).WithMessage(PayrollMessages.Reason);
        });
    }
}

public sealed class CreatePayrollAdjustmentCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<CreatePayrollAdjustmentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(CreatePayrollAdjustmentCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var input = command.Input;
            var employee = await db.Employees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.EmployeeId, cancellationToken);
            if (employee is null) return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var adjustment = PayrollAdjustment.Create(
                new PayrollEntryTarget(change.Period.Month, employee.Id, employee.FullName), input.Amount, input.Reason,
                change.ActorName, change.Now);
            if (adjustment.IsFailure) return adjustment.Error!.ToResult();
            db.PayrollAdjustments.Add(adjustment.Value);
            return null;
        }, cancellationToken);
}

/// <summary>Cancels an adjustment with a reason.</summary>
public sealed record VoidPayrollAdjustmentCommand(ProductionActor Actor, int AdjustmentId, VoidPayrollEntryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class VoidPayrollAdjustmentCommandValidator : AbstractValidator<VoidPayrollAdjustmentCommand>
{
    public VoidPayrollAdjustmentCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.AdjustmentId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Reason).Must(PayrollAmounts.IsReason).WithMessage(PayrollMessages.Reason)
            .When(x => x.Input is not null);
    }
}

public sealed class VoidPayrollAdjustmentCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<VoidPayrollAdjustmentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(VoidPayrollAdjustmentCommand command,
        CancellationToken cancellationToken)
    {
        var adjustment = await db.PayrollAdjustments.SingleOrDefaultAsync(x => x.Id == command.AdjustmentId,
            cancellationToken);
        if (adjustment is null) return Result<PayrollMonthResponse>.NotFound(PayrollMessages.AdjustmentNotFound);
        return await changes.ApplyAsync(command.Actor, PayrollMonths.Format(adjustment.Month), adminOnly: true, change =>
            Task.FromResult(adjustment.Void(command.Input.Reason, change.ActorName, change.Now).FailureOrNull()),
            cancellationToken);
    }
}

/// <summary>«Видано»: the sum handed to a worker for the month; null clears it.</summary>
public sealed record SetPayrollIssuedCommand(ProductionActor Actor, string Month, SetPayrollIssuedInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class SetPayrollIssuedCommandValidator : AbstractValidator<SetPayrollIssuedCommand>
{
    public SetPayrollIssuedCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.EmployeeId).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Amount)
            .Must(x => x is >= 0 and <= PayrollPayment.MaxAmount && PayrollAmounts.IsKopecks(x.Value))
            .WithMessage("Сума від 0 до 10 000 000 грн, з точністю до копійки.")
            .When(x => x.Input?.Amount is not null);
    }
}

public sealed class SetPayrollIssuedCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollIssuedCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollIssuedCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var input = command.Input;
            if (!await db.Employees.AnyAsync(x => x.Id == input.EmployeeId, cancellationToken))
                return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var month = change.Period.Month;
            var row = await db.PayrollIssued.SingleOrDefaultAsync(x => x.Month == month && x.EmployeeId == input.EmployeeId,
                cancellationToken);
            if (input.Amount is not { } amount)
            {
                if (row is not null) db.PayrollIssued.Remove(row);
                return null;
            }

            if (row is null)
            {
                row = new PayrollIssued(month, input.EmployeeId);
                db.PayrollIssued.Add(row);
            }

            return row.Set(amount, change.ActorName, change.Now).FailureOrNull();
        }, cancellationToken);
}
