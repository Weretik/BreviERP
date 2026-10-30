using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Documents.EmailTemplates;

public sealed class EmailTemplateVersion : BaseEntity<EmailTemplateVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public int Number { get; private set; }
    public string Name { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public CrmUserId VerifiedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private EmailTemplateVersion() { }

    public static EmailTemplateVersion Create(int number, string name, string subject, string body, CrmUserId actor)
    {
        if (number <= 0 || actor.Value <= 0) throw new EmailTemplateValidationException("invalid_email_template_actor_or_number");
        var version = new EmailTemplateVersion {
            Number = number, Name = Text(name, 120), Subject = Text(subject, 200),
            Body = Text(body, 3000), VerifiedBy = actor
        };
        if (version.Subject.Contains('\r') || version.Subject.Contains('\n'))
            throw new EmailTemplateValidationException("email_subject_header_invalid");
        ValidatePlaceholders(version.Subject);
        ValidatePlaceholders(version.Body);
        return version;
    }

    private static string Text(string? value, int maximum)
    {
        var text = value?.Trim();
        if (string.IsNullOrWhiteSpace(text) || text.Length > maximum || text.IndexOf('\0') >= 0)
            throw new EmailTemplateValidationException("email_template_text_invalid");
        return text;
    }

    private static void ValidatePlaceholders(string text)
    {
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '}') throw new EmailTemplateValidationException("email_template_placeholder_invalid");
            if (text[index] != '{') continue;
            var close = text.IndexOf('}', index + 1);
            if (close < 0 || text[(index + 1)..close] is not ("recipientName" or "managerName" or "quoteNumber"))
                throw new EmailTemplateValidationException("email_template_placeholder_invalid");
            index = close;
        }
    }
}
