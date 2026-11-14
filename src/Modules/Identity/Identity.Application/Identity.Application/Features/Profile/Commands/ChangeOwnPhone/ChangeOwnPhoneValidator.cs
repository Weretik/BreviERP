using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnPhone;

public sealed class ChangeOwnPhoneValidator : AbstractValidator<ChangeOwnPhoneCommand>
{
    public ChangeOwnPhoneValidator()
    {
        RuleFor(x => x.PhoneNumber).PhoneNumber();
    }
}
