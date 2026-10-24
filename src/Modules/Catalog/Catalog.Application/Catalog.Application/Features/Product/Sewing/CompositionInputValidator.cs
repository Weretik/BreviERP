using Catalog.Domain.Products.Entities;

namespace Catalog.Application.Features.Product.Sewing;

internal static class CompositionInputValidator
{
    internal static List<ValidationError> Validate(long expectedVersion, IReadOnlyList<SewingMaterialInput>? materials,
        IReadOnlyList<SewingOperationInput>? operations = null)
    {
        var errors = new List<ValidationError>();
        if (expectedVersion < 0) errors.Add(new("expectedVersion", "Version must be nonnegative."));
        if (materials is null || materials.Count > 2000)
            errors.Add(new("materials", "Provide at most 2000 material lines."));
        else for (var i = 0; i < materials.Count; i++)
        {
            var line = materials[i];
            var field = $"materials[{i}]";
            if (line is null) { errors.Add(new(field, "A material line is required.")); continue; }
            if (!Enum.TryParse<SewingMaterialKind>(line.Kind, out var kind) || !Enum.IsDefined(kind) || kind.ToString() != line.Kind)
                errors.Add(new(field + ".kind", "Unknown material kind."));
            if (!ValidDecimal(line.Quantity)) errors.Add(new(field + ".quantity", "Use a positive value with at most six decimal places."));
            if (line.SortOrder < 0) errors.Add(new(field + ".sortOrder", "Order must be nonnegative."));
            var unit = kind switch { SewingMaterialKind.Fabric => "Metre", SewingMaterialKind.Accessory => "Piece", SewingMaterialKind.OtherCost => "Service", _ => null };
            if (unit is null || line.Unit != unit) errors.Add(new(field + ".unit", "Unit is incompatible with this material kind."));
            if (kind == SewingMaterialKind.OtherCost)
            {
                if (line.ReferenceId.HasValue || string.IsNullOrWhiteSpace(line.CostCode) || line.CostCode.Trim().Length > 80)
                    errors.Add(new(field + ".costCode", "Other cost requires a cost code and no inventory reference."));
            }
            else if (line.ReferenceId is null or <= 0 || !string.IsNullOrWhiteSpace(line.CostCode))
                errors.Add(new(field + ".referenceId", "A positive material reference is required; cost code must be empty."));
        }
        if (operations is { Count: > 2000 }) errors.Add(new("operations", "Provide at most 2000 operation lines."));
        else if (operations is not null) for (var i = 0; i < operations.Count; i++)
        {
            var line = operations[i]; var field = $"operations[{i}]";
            if (line is null) { errors.Add(new(field, "An operation line is required.")); continue; }
            if (line.OperationId <= 0) errors.Add(new(field + ".operationId", "A positive operation reference is required."));
            if (!ValidDecimal(line.Quantity)) errors.Add(new(field + ".quantity", "Use a positive value with at most six decimal places."));
            if (line.ManualMinutes.HasValue && !ValidDecimal(line.ManualMinutes.Value)) errors.Add(new(field + ".manualMinutes", "Manual minutes must be positive with at most six decimal places."));
            if (line.SortOrder < 0) errors.Add(new(field + ".sortOrder", "Order must be nonnegative."));
        }
        return errors;
    }

    private static bool ValidDecimal(decimal value) => value > 0 && value <= 999_999_999_999.999999m && decimal.Round(value, 6) == value;
}
