using Host.Api.Crm.CustomSewing.Suggestions.Mapping;
using Crm.Application.CustomSewing.GetSuggestions.Contracts;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Suggestions;
using Crm.Application.CustomSewing.GetSuggestions;

namespace Host.Api.Crm.CustomSewing.Suggestions;

public sealed class SewingSuggestionSource(ISewingSuggestionIdentityReader identityReader, ISewingSuggestionCompositionReader compositions, IPricingReferenceVersionReader references)
    : ISewingSuggestionSource
{
    public async Task<SuggestionSource> ReadAsync(string productType, string season, CancellationToken ct)
    {
        var sourceSeason = LegacySewingSuggestionType.SourceSeason(season);
        if (!LegacySewingSuggestionType.IsSupported(productType) || sourceSeason is null) return new([], false);
        var batch = await identityReader.ReadAsync(sourceSeason, ct);
        // A truncated set could split a model group and incorrectly call it unique. Fail closed without partial norms.
        if (batch.LimitReached) return new([], true);
        var identities = batch.Identities.Where(x => LegacySewingSuggestionType.FromModelName(x.Name) == productType).ToArray();
        var compositionBatch = await compositions.ReadAsync(identities, sourceSeason, ct);
        if (compositionBatch.LimitReached) return new([], true);
        var snapshots = compositionBatch.Compositions.ToLookup(x => (x.ProductId, x.ArticleId));
        var keys = compositionBatch.Compositions.SelectMany(SewingSuggestionInputMapper.ReferenceKeys)
            .Distinct(StringComparer.Ordinal).Take(10001).ToArray();
        if (keys.Length > 10000) return new([], true);
        IReadOnlyList<PricingReferenceValue> resolved = keys.Length == 0 ? [] : await references.ReadAsync(keys, DateTimeOffset.UtcNow, ct);
        var referenceGroups = resolved.ToLookup(x => x.ReferenceKey, StringComparer.Ordinal);
        var result = new List<SuggestionInput>();
        foreach (var identity in identities)
        {
            var candidates = snapshots[(identity.ProductId, identity.ArticleId)].ToArray();
            var composition = candidates.Length == 1 ? candidates[0] : null;
            result.Add(SewingSuggestionInputMapper.Map(identity, productType, season, composition, referenceGroups));
        }
        var rejected = result.Where(x => !x.Approved).Select(x => x.ModelCode).ToHashSet(StringComparer.Ordinal);
        return new(result.Where(x => !rejected.Contains(x.ModelCode)).ToArray(), false);
    }
}
