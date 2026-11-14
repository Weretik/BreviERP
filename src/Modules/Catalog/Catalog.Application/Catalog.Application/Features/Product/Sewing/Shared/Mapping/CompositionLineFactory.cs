using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Sewing;

internal static class CompositionLineFactory
{
    /// <summary>The lines of a request; the first line a rule refuses stops it.</summary>
    internal static DomainResult<List<SewingMaterialLine>> Materials(IEnumerable<SewingMaterialInput> lines,
        ProductId productId)
    {
        var result = new List<SewingMaterialLine>();
        foreach (var line in lines)
        {
            var material = Material(line, productId, null);
            if (material.IsFailure) return material.Error!;
            result.Add(material.Value);
        }

        return result;
    }

    internal static DomainResult<List<SewingOperationLine>> Operations(IEnumerable<SewingOperationInput> lines,
        ProductId productId)
    {
        var result = new List<SewingOperationLine>();
        foreach (var line in lines)
        {
            var operation = Operation(line, productId);
            if (operation.IsFailure) return operation.Error!;
            result.Add(operation.Value);
        }

        return result;
    }

    internal static DomainResult<SewingMaterialLine> Material(SewingMaterialInput line, ProductId productId, ArticleId? articleId)
    {
        var kind = Enum.Parse<SewingMaterialKind>(line.Kind);
        return SewingMaterialLine.Create(default, productId, articleId, kind,
            kind == SewingMaterialKind.Fabric ? FabricReferenceId.Create(line.ReferenceId!.Value) : null,
            kind == SewingMaterialKind.Accessory ? AccessoryReferenceId.Create(line.ReferenceId!.Value) : null,
            line.CostCode, line.Quantity, Enum.Parse<SewingMaterialUnit>(line.Unit), line.SortOrder,
            line.Comment);
    }

    internal static DomainResult<SewingOperationLine> Operation(SewingOperationInput line, ProductId productId)
        => SewingOperationLine.Create(default, productId, null, OperationReferenceId.Create(line.OperationId),
            line.Quantity, line.ManualMinutes, line.SortOrder, line.Comment);
}

