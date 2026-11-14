using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Payroll;

namespace Production.Application.Payroll.Setup;

/// <summary>«Керівні частки»: who gets the master's and the chief's share; holds for every open month (PRD-D44).</summary>
public sealed record SetPayrollRolesCommand(ProductionActor Actor, string Month, SetPayrollRolesInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class SetPayrollRolesCommandValidator : AbstractValidator<SetPayrollRolesCommand>
{
    public SetPayrollRolesCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.MasterEmployeeId).GreaterThan(0).When(x => x.Input?.MasterEmployeeId is not null);
        RuleFor(x => x.Input.ChiefEmployeeId).GreaterThan(0).When(x => x.Input?.ChiefEmployeeId is not null);
    }
}

public sealed class SetPayrollRolesCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollRolesCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollRolesCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: false, async change =>
        {
            var ids = new[] { command.Input.MasterEmployeeId, command.Input.ChiefEmployeeId }.OfType<int>().ToList();
            var active = await db.Employees.Where(x => ids.Contains(x.Id) && x.IsActive).Select(x => x.Id)
                .ToListAsync(cancellationToken);
            if (ids.Except(active).Any())
                return Result.Invalid(new ValidationError("Input", PayrollMessages.ActiveEmployeeNotFound));
            var roles = await db.PayrollRoles.ToDictionaryAsync(x => x.Role, cancellationToken);
            foreach (var (kind, employeeId) in new[]
                     {
                         (PayrollRoleKind.Master, command.Input.MasterEmployeeId),
                         (PayrollRoleKind.Chief, command.Input.ChiefEmployeeId)
                     })
            {
                if (!roles.TryGetValue(kind, out var role))
                {
                    role = new PayrollRole(kind);
                    db.PayrollRoles.Add(role);
                }

                role.Assign(employeeId, change.ActorName, change.Now);
            }

            return null;
        }, cancellationToken);
}

/// <summary>«Вишивка» of the month: the stitches and who embroidered them.</summary>
public sealed record SetPayrollEmbroideryCommand(ProductionActor Actor, string Month, SetPayrollEmbroideryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class SetPayrollEmbroideryCommandValidator : AbstractValidator<SetPayrollEmbroideryCommand>
{
    public SetPayrollEmbroideryCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Stitches).InclusiveBetween(0, PayrollMonth.MaxStitches).WithMessage(PayrollMessages.Stitches)
            .When(x => x.Input is not null);
        RuleFor(x => x.Input.EmployeeId).GreaterThan(0).When(x => x.Input?.EmployeeId is not null);
    }
}

public sealed class SetPayrollEmbroideryCommandHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollEmbroideryCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollEmbroideryCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: false, async change =>
        {
            if (command.Input.EmployeeId is { } id &&
                !await db.Employees.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken))
                return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.ActiveEmployeeNotFound));
            return change.Period.SetEmbroidery(command.Input.Stitches, command.Input.EmployeeId, change.ActorName,
                change.Now).FailureOrNull();
        }, cancellationToken);
}
