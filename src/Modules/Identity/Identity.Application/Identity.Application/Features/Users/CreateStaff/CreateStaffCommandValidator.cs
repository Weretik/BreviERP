using FluentValidation;
using Identity.Application.Features.Profile.Shared;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.CreateStaff;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Role).Must(StaffRoles.IsStaffRole).WithMessage("Role must be Director, Admin, Accountant, Manager or WorkshopChief.");
        RuleFor(x => x.Password).StaffPassword();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
