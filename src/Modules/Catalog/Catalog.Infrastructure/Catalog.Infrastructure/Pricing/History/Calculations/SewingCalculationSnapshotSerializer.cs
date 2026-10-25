using Catalog.Application.Contracts.Pricing;

namespace Catalog.Infrastructure.Pricing.History.Calculations;

internal static class SewingCalculationSnapshotSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string Serialize(SewingArticlePricePreview snapshot)
        => JsonSerializer.Serialize(snapshot, Json);

    public static SavedSewingCalculation Read(SavedSewingCalculationRecord row)
        => new(row.PublicId, row.RequestId, row.CreatedAt, row.CreatedBy,
            JsonSerializer.Deserialize<SewingArticlePricePreview>(row.SnapshotJson, Json)!);
}
