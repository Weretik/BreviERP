using Catalog.Domain.Products.Enums;

namespace Catalog.Domain.Products.Readiness;

public static class SewingCompositionReadiness
{
    public static IReadOnlyList<string> Evaluate(
        bool articleIsActive,
        IEnumerable<MaterialReadinessFact> materials,
        IEnumerable<OperationReadinessFact> operations)
    {
        ArgumentNullException.ThrowIfNull(materials);
        ArgumentNullException.ThrowIfNull(operations);

        var materialFacts = materials.ToArray();
        var operationFacts = operations.ToArray();
        var blockers = new List<string>();

        if (!articleIsActive)
            blockers.Add("Article is inactive.");

        blockers.AddRange(materialFacts
            .Where(material => material.IsRequired && !material.IsReviewed)
            .Select(material => $"Required material line {material.Id.Value} has not been reviewed."));
        blockers.AddRange(operationFacts
            .Where(operation => !operation.IsReviewed)
            .Select(operation => $"Required operation line {operation.Id.Value} has not been reviewed."));
        blockers.AddRange(materialFacts
            .Where(material => material.Kind == SewingMaterialKind.OtherCost)
            .Select(material => $"Other cost line {material.Id.Value} requires the versioned Article pricing check."));

        if (!materialFacts.Any(material => material.Kind == SewingMaterialKind.Fabric))
            blockers.Add("No effective fabric line is configured.");
        if (operationFacts.Length == 0)
            blockers.Add("No operation lines are configured.");

        return blockers;
    }
}
