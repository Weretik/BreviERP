using FluentValidation;
using Identity.Application.Features.Profile.Shared;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.CreateStaff;

public sealed class CreateStaffValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Role).Must(StaffRoles.IsAssignable).WithMessage("Role must be a staff role from «Права доступу».");
        RuleFor(x => x.Password).StaffPassword();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
