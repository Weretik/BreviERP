using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Sewing;

internal static class SewingMaterialInputValidator
{
    internal static List<ValidationError> Validate(IReadOnlyList<SewingMaterialInput>? materials)
    {
        var errors = new List<ValidationError>();
        if (materials is null || materials.Count > 2_000)
        {
            errors.Add(new("materials", "Provide at most 2000 material lines."));
            return errors;
        }

        for (var index = 0; index < materials.Count; index++)
        {
            var line = materials[index];
            var field = $"materials[{index}]";
            if (line is null)
            {
                errors.Add(new(field, "A material line is required."));
                continue;
            }

            var validKind = Enum.TryParse<SewingMaterialKind>(line.Kind, out var kind)
                && Enum.IsDefined(kind) && kind.ToString() == line.Kind;
            if (!validKind)
                errors.Add(new(field + ".kind", "Unknown material kind."));
            if (!ValidDecimal(line.Quantity))
                errors.Add(new(field + ".quantity", "Use a positive value with at most six decimal places."));
            if (line.SortOrder < 0)
                errors.Add(new(field + ".sortOrder", "Order must be nonnegative."));

            var unit = kind switch
            {
                SewingMaterialKind.Fabric => "Metre",
                SewingMaterialKind.Accessory => "Piece",
                SewingMaterialKind.OtherCost => "Service",
                _ => null
            };
            if (unit is null || (line.Unit != unit
                && !(kind == SewingMaterialKind.Accessory && line.Unit == "Metre")))
                errors.Add(new(field + ".unit", "Unit is incompatible with this material kind."));

            if (kind == SewingMaterialKind.OtherCost)
            {
                if (line.ReferenceId.HasValue || string.IsNullOrWhiteSpace(line.CostCode)
                    || line.CostCode.Trim().Length > 80)
                    errors.Add(new(field + ".costCode", "Other cost requires a cost code and no inventory reference."));
            }
            else if (line.ReferenceId is null or <= 0 || !string.IsNullOrWhiteSpace(line.CostCode))
            {
                errors.Add(new(field + ".referenceId", "A positive material reference is required; cost code must be empty."));
            }
        }

        return errors;
    }

    private static bool ValidDecimal(decimal value)
        => value > 0 && value <= 999_999_999_999.999999m && decimal.Round(value, 6) == value;
}
