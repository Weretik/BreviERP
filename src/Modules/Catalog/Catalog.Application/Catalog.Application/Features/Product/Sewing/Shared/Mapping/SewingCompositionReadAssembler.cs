using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

internal static class SewingCompositionReadAssembler
{
    internal static SewingComposition Assemble(SewingCompositionReadSnapshot snapshot)
    {
        var materials = snapshot.Materials.Select(material => new MaterialReadinessFact(
            CompositionLineId.Create(material.Id),
            Enum.Parse<SewingMaterialKind>(material.Kind),
            material.Required,
            material.Reviewed));
        var operations = snapshot.Operations.Select(operation => new OperationReadinessFact(
            CompositionLineId.Create(operation.Id),
            operation.Reviewed));
        var blockers = SewingCompositionReadiness.Evaluate(snapshot.ArticleIsActive, materials, operations);

        return new SewingComposition(snapshot.ProductId, snapshot.ArticleId, snapshot.ModelCode, snapshot.Version,
            snapshot.Materials, snapshot.Operations, blockers);
    }
}
