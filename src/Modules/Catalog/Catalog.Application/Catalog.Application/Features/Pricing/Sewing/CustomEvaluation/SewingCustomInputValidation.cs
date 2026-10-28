using System.Globalization;
using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.CustomEvaluation;

internal static class SewingCustomInputValidation
{
    internal static bool IsValid(SewingCustomPricingInput input)
    {
        if (input.Materials is null || input.Operations is null || input.Charges is null
            || input.Materials.Count > 140 || input.Operations.Count > 100
            || input.Materials.Any(x => x is null) || input.Operations.Any(x => x is null))
            return false;
        return input.Materials.Select(x => x.LineId).Distinct().Count() == input.Materials.Count
            && input.Operations.Select(x => x.LineId).Distinct().Count() == input.Operations.Count
            && input.Materials.Count(x => x.IsFabric) <= 40
            && input.Materials.All(x => x.LineId > 0 && Norm(x.Quantity) && Reference(x)
                && !string.IsNullOrWhiteSpace(x.Unit) && x.Unit.Length <= 50)
            && input.Operations.All(x => x.LineId > 0 && x.OperationId > 0 && Norm(x.Quantity)
                && (x.ManualMinutes is null || Norm(x.ManualMinutes.Value)));
    }

    private static bool Norm(decimal value) => value > 0 && value <= 100000
        && decimal.Round(value, 6) == value;

    private static bool Reference(SewingPricingMaterialRow row)
    {
        if (row.ReferenceKey is null) return false;
        var prefix = row.IsOtherCost ? "OtherCost:" : row.IsFabric ? "Fabric:" : "Accessory:";
        if (row.IsOtherCost && row.IsFabric || !row.ReferenceKey.StartsWith(prefix, StringComparison.Ordinal))
            return false;
        var key = row.ReferenceKey[prefix.Length..];
        if (row.IsOtherCost)
            return key.Length is > 0 and <= 100
                && key.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');
        return int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            && id > 0 && id.ToString(CultureInfo.InvariantCulture) == key;
    }
}
