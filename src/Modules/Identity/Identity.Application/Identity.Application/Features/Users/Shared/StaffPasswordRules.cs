using FluentValidation;

namespace Identity.Application.Features.Users.Shared;

public static class StaffPasswordRules
{
    public const int MinimumLength = 8;

    public static IRuleBuilderOptions<T, string> StaffPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MinimumLength(MinimumLength).MaximumLength(128);
}
