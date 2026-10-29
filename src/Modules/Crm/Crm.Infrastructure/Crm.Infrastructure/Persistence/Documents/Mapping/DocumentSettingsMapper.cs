using Crm.Application.Documents.Contracts;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;

namespace Crm.Infrastructure.Persistence.Documents;

internal static class DocumentSettingsMapper
{
    public static IssuerSettingsDetail Detail(long revision, bool active, IssuerSettingsVersion issuer,
        DocumentTemplateVersion template)
        => new(revision, active, issuer.PublicId, template.PublicId, issuer.Number,
            new IssuerSettingsInput(issuer.LegalName, issuer.Edrpou, issuer.TaxId, issuer.Iban, issuer.Address,
                issuer.Phone, issuer.Email, issuer.Site, issuer.BankName, issuer.Mfo, issuer.LogoAssetKey),
            new DocumentTemplateInput(template.LayoutKey, template.Title, template.VatNotice, template.OpeningText,
                template.CtaTitle, template.CtaText, template.CtaContactTemplate), issuer.CreatedAt);
}
