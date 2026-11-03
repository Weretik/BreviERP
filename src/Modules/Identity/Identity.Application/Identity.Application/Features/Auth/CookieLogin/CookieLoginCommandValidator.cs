using FluentValidation;

namespace Identity.Application.Features.Auth.CookieLogin;

public sealed class CookieLoginCommandValidator : AbstractValidator<CookieLoginCommand>
{
    public CookieLoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
