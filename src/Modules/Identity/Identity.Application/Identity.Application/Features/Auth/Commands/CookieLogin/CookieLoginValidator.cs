using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.CookieLogin;

public sealed class CookieLoginValidator : AbstractValidator<CookieLoginCommand>
{
    public CookieLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
