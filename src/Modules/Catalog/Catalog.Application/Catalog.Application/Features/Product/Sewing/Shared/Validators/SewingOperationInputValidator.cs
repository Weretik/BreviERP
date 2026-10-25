namespace Catalog.Application.Features.Product.Sewing;

internal static class SewingOperationInputValidator
{
    internal static List<ValidationError> Validate(IReadOnlyList<SewingOperationInput>? operations)
    {
        var errors = new List<ValidationError>();
        if (operations is null)
        {
            return errors;
        }
        if (operations.Count > 2_000)
        {
            errors.Add(new("operations", "Provide at most 2000 operation lines."));
            return errors;
        }

        for (var index = 0; index < operations.Count; index++)
        {
            var line = operations[index];
            var field = $"operations[{index}]";
            if (line is null)
            {
                errors.Add(new(field, "An operation line is required."));
                continue;
            }

            if (line.OperationId <= 0)
                errors.Add(new(field + ".operationId", "A positive operation reference is required."));
            if (!ValidDecimal(line.Quantity))
                errors.Add(new(field + ".quantity", "Use a positive value with at most six decimal places."));
            if (line.ManualMinutes.HasValue && !ValidDecimal(line.ManualMinutes.Value))
                errors.Add(new(field + ".manualMinutes", "Manual minutes must be positive with at most six decimal places."));
            if (line.SortOrder < 0)
                errors.Add(new(field + ".sortOrder", "Order must be nonnegative."));
        }

        return errors;
    }

    private static bool ValidDecimal(decimal value)
        => value > 0 && value <= 999_999_999_999.999999m && decimal.Round(value, 6) == value;
}
