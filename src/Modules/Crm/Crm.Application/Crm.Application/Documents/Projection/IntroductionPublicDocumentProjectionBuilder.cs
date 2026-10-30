using Crm.Application.Abstractions.Documents;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Documents.Projection;

/// <summary>Builds only public, pinned introduction fields; it never reads a live catalog.</summary>
public static class IntroductionPublicDocumentProjectionBuilder
{
    public static IntroductionPublicSnapshot Freeze(IntroductionPreparedDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var slots = draft.Slots.Select(x => new IntroductionPublicSlotSnapshot(
            x.Selection.Slot, x.Selection.ProductId, x.Selection.ArticleId,
            x.ModelName, x.FabricName, x.FabricComposition, x.FabricDensity,
            x.ProductUrl, x.LargeWholesale)).ToArray();
        var snapshot = new IntroductionPublicSnapshot(draft.Layout, draft.MappingVersionId,
            draft.EmailTemplateVersionId, Array.AsReadOnly(slots));
        Validate(snapshot);
        return snapshot;
    }

    public static PublicDocumentProjection Build(IntroductionPublicSnapshot snapshot,
        IssuerSettingsVersion issuer, DocumentTemplateVersion template,
        string number, string recipientName, string managerName, PublicDocumentImage? logo = null)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(template);
        Validate(snapshot);
        if (string.IsNullOrWhiteSpace(number) || number.Length > 80 ||
            string.IsNullOrWhiteSpace(recipientName) || recipientName.Length > 200 ||
            string.IsNullOrWhiteSpace(managerName) || managerName.Length > 200 ||
            (issuer.LogoAssetKey is null) != (logo is null))
            throw new InvalidOperationException("introduction_public_header_invalid");

        var pages = IntroductionSlot.Required(snapshot.Layout)
            .GroupBy(x => x.Season)
            .Select(season => new PublicDocumentPage(
                season.Key == IntroductionSeason.Summer ? "Літні моделі" : "Зимові моделі",
                [$"Для: {recipientName.Trim()}", template.OpeningText],
                season.Select(slot => snapshot.Slots.Single(x => x.Slot == slot))
                    .Select(x => new PublicDocumentProductCard(x.ModelName, x.FabricName,
                        x.FabricComposition, x.FabricDensity, x.ProductUrl, x.LargeWholesale))
                    .ToArray(),
                [template.VatNotice,
                    $"{template.CtaTitle}: {template.CtaText}",
                    template.CtaContactTemplate.Replace("{managerName}", managerName.Trim(), StringComparison.Ordinal),
                    $"{issuer.LegalName} · {issuer.Phone} · {issuer.Email}"]))
            .ToArray();
        var document = new PublicDocumentProjection($"{template.Title} № {number.Trim()}", [], [], logo, pages);
        IntroductionDocumentLayoutPolicy.Validate(document);
        return document;
    }

    private static void Validate(IntroductionPublicSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.MappingVersionId == Guid.Empty || snapshot.EmailTemplateVersionId == Guid.Empty ||
            snapshot.Slots is null)
            throw new InvalidOperationException("introduction_public_snapshot_invalid");
        var required = IntroductionSlot.Required(snapshot.Layout);
        if (snapshot.Slots.Count != required.Count ||
            snapshot.Slots.Any(x => x is null || !required.Contains(x.Slot) ||
                x.ProductId <= 0 || x.ArticleId <= 0 || x.LargeWholesale <= 0 ||
                !ValidText(x.ModelName) || !ValidText(x.FabricName) ||
                !ValidText(x.FabricComposition) || !ValidText(x.FabricDensity) ||
                !IsBreviProductUrl(x.ProductUrl)) ||
            snapshot.Slots.Select(x => x.Slot).Distinct().Count() != required.Count ||
            snapshot.Slots.GroupBy(x => x.Slot.Season).Any(g =>
                g.Select(x => x.ProductId).Distinct().Count() != g.Count()))
            throw new InvalidOperationException("introduction_public_snapshot_invalid");
    }

    private static bool ValidText(string? text) =>
        !string.IsNullOrWhiteSpace(text) && text.Length <= 500;

    private static bool IsBreviProductUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var url) &&
        url.Scheme == Uri.UriSchemeHttps && url.UserInfo.Length == 0 &&
        url.AbsolutePath is not ("" or "/") &&
        (url.Host.Equals("brevi.com.ua", StringComparison.OrdinalIgnoreCase) ||
         url.Host.EndsWith(".brevi.com.ua", StringComparison.OrdinalIgnoreCase));
}
