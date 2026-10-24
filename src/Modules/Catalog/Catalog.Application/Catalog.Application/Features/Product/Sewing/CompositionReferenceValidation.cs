using Catalog.Application.Contracts.Reference;

namespace Catalog.Application.Features.Product.Sewing;

internal static class CompositionReferenceValidation
{
    internal static string? MissingReference(IReadOnlyList<SewingMaterialInput> materials,
        IReadOnlyList<SewingOperationInput> operations, ProductReferenceData data)
    {
        for (var i = 0; i < materials.Count; i++)
        {
            var line = materials[i];
            if (line.Kind == "Fabric" && !data.Fabrics.ContainsKey(line.ReferenceId!.Value) ||
                line.Kind == "Accessory" && !data.GarmentAccessories.ContainsKey(line.ReferenceId!.Value))
                return $"materials[{i}].referenceId: reference not found.";
        }
        for (var i = 0; i < operations.Count; i++)
            if (!data.GarmentPartOperations.ContainsKey(operations[i].OperationId))
                return $"operations[{i}].operationId: reference not found.";
        return null;
    }
}
