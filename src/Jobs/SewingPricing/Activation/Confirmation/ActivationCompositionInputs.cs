using Catalog.Application.Features.Product.Sewing;

namespace SewingPricing.Jobs.Activation;

public static class ActivationCompositionInputs
{
    public static SewingMaterialInput Material(SewingMaterialView line) => new(line.Kind, line.ReferenceId,
        line.CostCode, line.Quantity, line.Unit, line.Required, line.SortOrder, line.Reviewed);
    public static SewingOperationInput Operation(SewingOperationView line) => new(line.OperationId, line.Quantity,
        line.ManualMinutes, line.SortOrder, line.Reviewed);
    public static bool MaterialsEqual(IEnumerable<SewingMaterialInput> expected, IEnumerable<SewingMaterialInput> actual) =>
        expected.OrderBy(x => x.SortOrder).Select(x => x with { Reviewed = false })
            .SequenceEqual(actual.OrderBy(x => x.SortOrder).Select(x => x with { Reviewed = false }));
    public static bool OperationsEqual(IEnumerable<SewingOperationInput> expected, IEnumerable<SewingOperationInput> actual) =>
        expected.OrderBy(x => x.SortOrder).Select(x => x with { Reviewed = false })
            .SequenceEqual(actual.OrderBy(x => x.SortOrder).Select(x => x with { Reviewed = false }));
}
