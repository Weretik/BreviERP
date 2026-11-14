using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.SetPayrollRoles;

public sealed class SetPayrollRolesHandler(IProductionDbContext db, PayrollChanges changes)
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
