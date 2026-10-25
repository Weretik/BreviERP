using CatalogImport.Planning;
using CatalogImport.Source;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Pricing;

namespace SewingPricing.Jobs.Bootstrap;

public static class PricingReferenceBootstrapBuilder
{
    public static IReadOnlyList<ReferencePricingInputVersion> Build(
        WorkbookSnapshot snapshot, IReadOnlyDictionary<string, long> mappings, string hash, DateTimeOffset effectiveFrom)
    {
        int Id(string dataset, string key) => checked((int)(mappings.TryGetValue(dataset + ":" + key, out var value)
            ? value : throw new InvalidDataException($"Missing imported mapping: {dataset}:{key}.")));
        var versions = new List<ReferencePricingInputVersion>();
        foreach (var row in CatalogSourceReferences.Fabrics(snapshot))
            versions.Add(ReferencePricingInputVersion.Create(FabricId.From(Id("Fabric", row.Text(0))), null, null, null,
                row.Decimal(5), PricingInputUnit.UahPerMetre, row.Flag(7), true, 1,
                effectiveFrom, row.Text(0), hash, row.Text(6)));
        foreach (var row in snapshot.Rows("Фурнітура"))
        {
            var service = row.Text(4) == "послуга";
            var unit = row.Text(4) switch
            {
                "м" => PricingInputUnit.UahPerMetre,
                "шт." => PricingInputUnit.UahPerPiece,
                "послуга" => PricingInputUnit.UahPerService,
                _ => throw new InvalidDataException($"Unsupported source unit at {row.Key}.")
            };
            versions.Add(ReferencePricingInputVersion.Create(null,
                service ? null : GarmentAccessoryId.From(Id("Accessory", row.Text(0))), null,
                service ? row.Text(0) : null, row.Decimal(3), unit, row.Flag(6), true, 1,
                effectiveFrom, row.Text(0), hash, row.Text(5)));
        }
        foreach (var row in snapshot.Rows("Операції"))
            versions.Add(ReferencePricingInputVersion.Create(null, null, GarmentPartOperationId.From(Id("Operation", row.Text(0))),
                null, row.Decimal(5), PricingInputUnit.Minutes, row.Flag(9), true, 1,
                effectiveFrom, row.Text(0), hash, row.Text(8)));
        foreach (var row in snapshot.Rows("Трудовитрати виробів", 1).Where(row => row.Text(5).Length == 0))
        {
            var key = CatalogSourceIdentity.OperationKey(row);
            versions.Add(ReferencePricingInputVersion.Create(null, null, GarmentPartOperationId.From(Id("Operation", key)),
                null, null, PricingInputUnit.Minutes, true, true, 1, effectiveFrom, key, hash, null));
        }
        return versions;
    }
}

