using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.ChangeOwnPhone;

public sealed class ChangeOwnPhoneCommandValidator : AbstractValidator<ChangeOwnPhoneCommand>
{
    public ChangeOwnPhoneCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
