using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.ChangeStaffRole;

public sealed class ChangeStaffRoleValidator : AbstractValidator<ChangeStaffRoleCommand>
{
    public ChangeStaffRoleValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Role).Must(StaffRoles.IsAssignable).WithMessage("Role must be a staff role from «Права доступу».");
    }
}
