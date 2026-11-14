using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Inputs;
using Catalog.Application.Features.Product.Shared.Sewing.Views;

namespace SewingPricing.Jobs.Activation;

public static class ActivationCompositionInputs
{
    public static SewingMaterialInput Material(SewingMaterialView line) => new(line.Kind, line.ReferenceId,
        line.CostCode, line.Quantity, line.Unit, line.SortOrder);
    public static SewingOperationInput Operation(SewingOperationView line) => new(line.OperationId, line.Quantity,
        line.ManualMinutes, line.SortOrder);
    public static bool MaterialsEqual(IEnumerable<SewingMaterialInput> expected, IEnumerable<SewingMaterialInput> actual) =>
        expected.OrderBy(x => x.SortOrder).SequenceEqual(actual.OrderBy(x => x.SortOrder));
    public static bool OperationsEqual(IEnumerable<SewingOperationInput> expected, IEnumerable<SewingOperationInput> actual) =>
        expected.OrderBy(x => x.SortOrder).SequenceEqual(actual.OrderBy(x => x.SortOrder));
}
