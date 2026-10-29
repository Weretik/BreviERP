using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Documents.Templates;

public sealed class DocumentTemplateVersion : BaseEntity<DocumentTemplateVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public int Number { get; private set; }
    public string LayoutKey { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string VatNotice { get; private set; } = null!;
    public string OpeningText { get; private set; } = null!;
    public string CtaTitle { get; private set; } = null!;
    public string CtaText { get; private set; } = null!;
    public string CtaContactTemplate { get; private set; } = null!;
    public CrmUserId VerifiedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private DocumentTemplateVersion() { }

    public static DocumentTemplateVersion Create(int number, string layoutKey, string title, string vatNotice,
        string openingText, string ctaTitle, string ctaText, string ctaContactTemplate, CrmUserId verifiedBy)
    {
        if (number <= 0 || verifiedBy.Value <= 0 || layoutKey != "standard")
            throw new DocumentSettingsValidationException("invalid_document_layout");
        return new DocumentTemplateVersion
        {
            Number = number, LayoutKey = layoutKey,
            Title = Required(title, 200, "title_required"),
            VatNotice = Required(vatNotice, 300, "tax_wording_required"),
            OpeningText = Required(openingText, 3000, "opening_text_required"),
            CtaTitle = Required(ctaTitle, 500, "cta_title_required"),
            CtaText = Required(ctaText, 3000, "cta_text_required"),
            CtaContactTemplate = Required(ctaContactTemplate, 2000, "cta_contact_required"),
            VerifiedBy = verifiedBy
        };
    }

    private static string Required(string? value, int max, string code)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > max)
            throw new DocumentSettingsValidationException(code);
        return trimmed;
    }
}
