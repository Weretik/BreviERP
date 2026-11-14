using Catalog.Domain.Products;
using Catalog.Domain.Products.Readiness;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.Product.Shared.Sewing;

namespace Catalog.Application.Features.Product.Shared.Sewing.Mapping;

internal static class SewingCompositionReadAssembler
{
    internal static SewingComposition Assemble(SewingCompositionReadSnapshot snapshot)
    {
        var materials = snapshot.Materials.Select(material => new MaterialReadinessFact(
            CompositionLineId.Create(material.Id),
            Enum.Parse<SewingMaterialKind>(material.Kind)));
        var operations = snapshot.Operations.Select(operation => new OperationReadinessFact(
            CompositionLineId.Create(operation.Id)));
        var blockers = SewingCompositionReadiness.Evaluate(snapshot.ArticleIsActive, materials, operations);

        return new SewingComposition(snapshot.ProductId, snapshot.ArticleId, snapshot.ModelCode, snapshot.Version,
            snapshot.Materials, snapshot.Operations, blockers);
    }
}
