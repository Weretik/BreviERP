using Ardalis.Result;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.BuildIntroduction;

public sealed class IntroductionDraftBuilder(IIntroductionCatalogReader catalog)
{
    public async Task<Result<IntroductionPreparedDraft>> BuildAsync(
        IntroductionSelectionSnapshot snapshot, FeaturedModelMapping currentMapping,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(currentMapping);
        if (snapshot.MappingVersionId != currentMapping.VersionId)
            return Result<IntroductionPreparedDraft>.Conflict("featured_mapping_changed");

        var errors = new List<ValidationError>();
        var resolved = new List<IntroductionPreparedSlot>(snapshot.Selections.Count);
        foreach (var selection in snapshot.Selections)
        {
            var field = $"slots.{selection.Slot.Uid}";
            var offer = await catalog.ReadAsync(selection.ProductId, selection.ArticleId,
                cancellationToken);
            if (offer is null)
            {
                errors.Add(new ValidationError(field, "selection_unavailable"));
                continue;
            }
            var slotErrorCount = errors.Count;
            if (offer.Season != selection.Slot.Season || offer.Kind != "made" ||
                !offer.Approved || !offer.Sellable)
                errors.Add(new ValidationError(field, "model_unavailable_or_wrong_season"));
            if (string.IsNullOrWhiteSpace(offer.ModelName) ||
                string.IsNullOrWhiteSpace(offer.FabricName) ||
                string.IsNullOrWhiteSpace(offer.FabricComposition) ||
                string.IsNullOrWhiteSpace(offer.FabricDensity))
                errors.Add(new ValidationError(field, "fabric_metadata_missing"));
            if (!IsBreviProductUrl(offer.ProductUrl))
                errors.Add(new ValidationError(field, "product_url_invalid"));

            var price = offer.CurrentPrice;
            if (price is null || price.Kind != "sewing" ||
                price.ProductId != selection.ProductId ||
                price.ArticleId != selection.ArticleId ||
                price.SourceId == Guid.Empty || price.Blockers is null ||
                price.Blockers.Count > 0 || price.LargeWholesale is not > 0)
            {
                errors.Add(new ValidationError(field, "large_wholesale_price_unavailable"));
                continue;
            }
            if (errors.Count != slotErrorCount) continue;
            resolved.Add(new IntroductionPreparedSlot(selection,
                offer.ModelName.Trim(), offer.FabricName.Trim(), offer.FabricComposition.Trim(),
                offer.FabricDensity.Trim(), offer.ProductUrl, price.LargeWholesale.Value, price));
        }

        if (errors.Count > 0) return Result<IntroductionPreparedDraft>.Invalid(errors);
        return Result.Success(new IntroductionPreparedDraft(snapshot.Layout,
            snapshot.MappingVersionId, snapshot.EmailTemplateVersionId, resolved.AsReadOnly()));
    }

    private static bool IsBreviProductUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || url.UserInfo.Length > 0 ||
            url.AbsolutePath is "" or "/") return false;
        return url.Host.Equals("brevi.com.ua", StringComparison.OrdinalIgnoreCase) ||
            url.Host.EndsWith(".brevi.com.ua", StringComparison.OrdinalIgnoreCase);
    }
}
