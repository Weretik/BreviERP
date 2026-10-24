using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

internal static class CompositionLineFactory
{
    internal static SewingMaterialLine Material(SewingMaterialInput line, ProductId productId, ArticleId? articleId,
        string actor, DateTimeOffset now)
    {
        var kind = Enum.Parse<SewingMaterialKind>(line.Kind);
        return SewingMaterialLine.Create(default, productId, articleId, kind,
            kind == SewingMaterialKind.Fabric ? FabricReferenceId.Create(line.ReferenceId!.Value) : null,
            kind == SewingMaterialKind.Accessory ? AccessoryReferenceId.Create(line.ReferenceId!.Value) : null,
            line.CostCode, line.Quantity, Enum.Parse<SewingMaterialUnit>(line.Unit), line.Required, line.SortOrder,
            line.Reviewed, line.Reviewed ? actor : null, line.Reviewed ? now : null);
    }

    internal static SewingOperationLine Operation(SewingOperationInput line, ProductId productId, string actor, DateTimeOffset now)
        => SewingOperationLine.Create(default, productId, null, OperationReferenceId.Create(line.OperationId), line.Quantity,
            line.ManualMinutes, line.SortOrder, true, line.Reviewed, line.Reviewed ? actor : null, line.Reviewed ? now : null);
}
