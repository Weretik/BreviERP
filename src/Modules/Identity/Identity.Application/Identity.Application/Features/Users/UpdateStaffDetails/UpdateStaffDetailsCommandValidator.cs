using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.UpdateStaffDetails;

public sealed class UpdateStaffDetailsCommandValidator : AbstractValidator<UpdateStaffDetailsCommand>
{
    public UpdateStaffDetailsCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.FullName).FullName();
        RuleFor(x => x.Email).StaffEmail();
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
