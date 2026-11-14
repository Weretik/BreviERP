using Crm.Domain.Quotes.Sending;
using Crm.Application.Features.Quotes.Shared.Sending;

namespace Crm.Application.Features.Quotes.Commands.SendQuote;

public sealed class SendQuoteValidator : AbstractValidator<SendQuoteCommand>
{
    public SendQuoteValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().ChildRules(input =>
        {
            input.RuleFor(x => x.Version).GreaterThan(0);
            input.RuleFor(x => x.To).NotEmpty().WithMessage("Вкажіть email отримувача.")
                .MaximumLength(QuoteSendingLimits.RecipientsMaxLength);
            input.RuleFor(x => QuoteRecipients.Parse(x.To))
                .Must(x => x.Length <= QuoteSendingLimits.MaxRecipients)
                .WithMessage($"Не більше {QuoteSendingLimits.MaxRecipients} отримувачів.")
                .Must(x => x.All(IsEmail)).WithMessage("Перевірте email отримувача.")
                .OverridePropertyName("To")
                .When(x => !string.IsNullOrWhiteSpace(x.To));
            input.RuleFor(x => x.Subject).NotEmpty().WithMessage("Вкажіть тему листа.")
                .MaximumLength(QuoteSendingLimits.SubjectMaxLength)
                .Must(x => !x.Contains('\r') && !x.Contains('\n')).WithMessage("Тема — один рядок.");
            input.RuleFor(x => x.Body).NotEmpty().WithMessage("Напишіть текст листа.")
                .MaximumLength(QuoteSendingLimits.BodyMaxLength);
        });
    }

    /// <summary>A plain address: one @, a dot in the domain, no spaces, quotes or angle brackets.</summary>
    internal static bool IsEmail(string value) =>
        value.Length <= QuoteSendingLimits.EmailMaxLength
        && System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s""<>,;]+@[^@\s""<>,;]+\.[^@\s""<>,;]+$");
}
