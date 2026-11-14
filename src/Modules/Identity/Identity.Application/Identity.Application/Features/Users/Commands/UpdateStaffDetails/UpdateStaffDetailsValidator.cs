using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.Commands.UpdateStaffDetails;

public sealed class UpdateStaffDetailsValidator : AbstractValidator<UpdateStaffDetailsCommand>
{
    public UpdateStaffDetailsValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.FullName).FullName();
        RuleFor(x => x.Email).StaffEmail();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
