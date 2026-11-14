using Catalog.Application.Abstractions.Pricing;
using Catalog.Infrastructure.Persistence.Configurations.Pricing.Calculations;
using Catalog.Application.Abstractions.Pricing.Calculations;


namespace Catalog.Infrastructure.Persistence.Repositories.Pricing.Calculations;

internal static class SewingCalculationSnapshotSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string Serialize(SewingArticlePricePreview snapshot)
        => JsonSerializer.Serialize(snapshot, Json);

    public static SavedSewingCalculation Read(SavedSewingCalculationRecord row)
        => new(row.PublicId, row.RequestId, row.CreatedAt, row.CreatedBy,
            JsonSerializer.Deserialize<SewingArticlePricePreview>(row.SnapshotJson, Json)!);
}
