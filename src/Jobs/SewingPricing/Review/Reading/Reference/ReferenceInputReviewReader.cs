using CatalogImport.Source;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.Reading;
using SewingPricing.Jobs.Bootstrap;
using SewingPricing.Jobs.Reading;

namespace SewingPricing.Jobs.Review;

internal static class ReferenceInputReviewReader
{
    public static async Task<IReadOnlyList<PricingReviewSubject>> ReadAsync(ReferenceDbContext reference,
        WorkbookSnapshot snapshot, IReadOnlyDictionary<string, long> mappings, PricingBootstrapSource source,
        DateTimeOffset at, CancellationToken cancellationToken)
    {
        var subjects = new List<PricingReviewSubject>();
        var expected = PricingReferenceBootstrapBuilder.Build(snapshot, mappings, source.CatalogSnapshotHash, source.CapturedAt);
        var actual = await new JobPricingReferenceReader(new ReferencePricingVersionReader(reference))
            .ReadAsync(expected.Select(Key).ToArray(), at, cancellationToken);
        foreach (var value in expected)
        {
            var candidates = actual.Where(x => x.ReferenceKey == Key(value)).ToArray();
            var selected = candidates.Length == 1 ? candidates[0] : null;
            var unit = value.Unit switch
            {
                PricingInputUnit.UahPerMetre => "Metre",
                PricingInputUnit.UahPerPiece => "Piece",
                PricingInputUnit.UahPerService => "Service",
                PricingInputUnit.Minutes => "Minutes",
                _ => throw new InvalidDataException("Unsupported source input unit.")
            };
            subjects.Add(new(value.SourceCode, "ReferenceInput", selected?.VersionId.ToString(),
                PricingReviewComparison.Fields(new { value.Value, Unit = unit, value.IsActive, value.IsApproved, value.SourceCode },
                    selected is null ? null : new { selected.Value, selected.Unit, selected.IsActive, selected.IsApproved, selected.SourceCode })));
        }
        return subjects;
    }

    private static string Key(ReferencePricingInputVersion value) => value.FabricId.HasValue ? $"Fabric:{value.FabricId.Value.Value}"
        : value.AccessoryId.HasValue ? $"Accessory:{value.AccessoryId.Value.Value}"
        : value.OperationId.HasValue ? $"Operation:{value.OperationId.Value.Value}" : $"OtherCost:{value.OtherCostCode}";
}
