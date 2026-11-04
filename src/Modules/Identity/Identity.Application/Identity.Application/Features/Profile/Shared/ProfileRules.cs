using FluentValidation;

namespace Identity.Application.Features.Profile.Shared;

public static class ProfileRules
{
    public const int FullNameMaxLength = 100;
    public const int EmailMaxLength = 256;

    public static IRuleBuilderOptions<T, string> FullName<T>(this IRuleBuilder<T, string> rule) =>
        rule.Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Name is required.")
            .Must(value => value is null || value.Trim().Length <= FullNameMaxLength)
            .WithMessage($"Name must be at most {FullNameMaxLength} characters.");

    public const int PhoneMaxLength = 20;

    /// <summary>Optional; digits with an optional leading +, spaces, dashes and brackets, 7 to 15 digits.</summary>
    public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => string.IsNullOrWhiteSpace(value) || IsPhone(value.Trim()))
            .WithMessage("Phone must contain 7 to 15 digits, optionally with +, spaces, dashes or brackets.");

    private static bool IsPhone(string value)
    {
        if (value.Length > PhoneMaxLength) return false;
        var digits = value.Count(char.IsAsciiDigit);
        var allowed = value.Select((c, i) => char.IsAsciiDigit(c) || c is ' ' or '-' or '(' or ')' || (c == '+' && i == 0)).All(ok => ok);
        return allowed && digits is >= 7 and <= 15;
    }

    public static IRuleBuilderOptions<T, string> StaffEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().EmailAddress().MaximumLength(EmailMaxLength);
}
