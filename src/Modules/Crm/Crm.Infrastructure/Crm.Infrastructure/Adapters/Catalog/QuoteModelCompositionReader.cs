using Catalog.Contracts.Sewing;
using Crm.Application.Quotes.CustomSewing;
using Crm.Domain.Quotes;

namespace Crm.Infrastructure.Adapters.Catalog;

/// <summary>
/// Reads the composition of a Catalog sewing model from its sewing card: the fabric of each variant and the shared
/// accessories, services, extra fabrics and operations.
/// </summary>
internal sealed class QuoteModelCompositionReader(ISewingModelReader models) : IQuoteModelCompositionReader
{
    public async Task<QuoteModelComposition?> ReadAsync(int productId, CancellationToken cancellationToken)
    {
        var model = await models.FindAsync(productId, cancellationToken);
        if (model is null) return null;
        var fabrics = model.Articles
            .Where(x => x.FabricId is > 0 && x.FabricQuantity is > 0)
            .OrderBy(x => x.SortOrder)
            .Select(x => new QuoteModelFabric(x.Id, x.FabricId!.Value, x.FabricName, x.FabricQuantity!.Value, x.Active))
            .ToArray();
        var materials = model.Materials.OrderBy(x => x.SortOrder).Select(Material).OfType<QuoteModelLine>();
        var operations = model.Operations.OrderBy(x => x.SortOrder).Select(x =>
            new QuoteModelLine(QuoteItemLineKind.Operation, x.OperationId, null, x.Name, "хв", x.Quantity,
                x.ManualMinutes));
        return new QuoteModelComposition(model.ProductId, fabrics, materials.Concat(operations).ToArray());
    }

    private static QuoteModelLine? Material(SewingModelMaterial line) => line.Kind switch
    {
        "Fabric" when line.ReferenceId is > 0 =>
            new(QuoteItemLineKind.Fabric, line.ReferenceId.Value, null, line.Name, Unit(line.Unit), line.Quantity, null),
        "Accessory" when line.ReferenceId is > 0 =>
            new(QuoteItemLineKind.Accessory, line.ReferenceId.Value, null, line.Name, Unit(line.Unit), line.Quantity,
                null),
        "OtherCost" when !string.IsNullOrWhiteSpace(line.CostCode) =>
            new(QuoteItemLineKind.Service, 0, line.CostCode.Trim(), line.CostCode.Trim(), Unit(line.Unit),
                line.Quantity, null),
        _ => null
    };

    private static string Unit(string unit) => unit switch
    {
        "Metre" => "м",
        "Piece" => "шт.",
        "Service" => "послуга",
        "Kilogram" => "кг",
        _ => unit
    };
}
