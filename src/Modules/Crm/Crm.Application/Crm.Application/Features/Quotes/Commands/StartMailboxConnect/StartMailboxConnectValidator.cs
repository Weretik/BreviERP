namespace Crm.Application.Features.Quotes.Commands.StartMailboxConnect;

public sealed class StartMailboxConnectValidator : AbstractValidator<StartMailboxConnectCommand>
{
    public StartMailboxConnectValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ReturnPath).NotEmpty().MaximumLength(200)
            .Must(MailboxReturnPath.IsAllowed).WithMessage("Невірна сторінка повернення.");
    }
}

/// <summary>Only Admin pages of the «Менеджер сервісу» block; never another site.</summary>
internal static class MailboxReturnPath
{
    public const string Fallback = "/crm/quote-settings";

    public static bool IsAllowed(string? path) =>
        path is not null && path.StartsWith("/crm/", StringComparison.Ordinal) && !path.Contains("//", StringComparison.Ordinal)
        && !path.Contains('\\') && path.All(c => c > ' ' && c < 127);
}
