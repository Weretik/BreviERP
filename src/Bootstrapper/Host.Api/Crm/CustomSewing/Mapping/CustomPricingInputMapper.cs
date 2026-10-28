using Catalog.Application.Contracts.Pricing;
using Crm.Domain.CustomSewing.Composition;

namespace Host.Api.Crm.CustomSewing.Mapping;

internal static class CustomPricingInputMapper
{
    internal static SewingCustomPricingInput Map(CustomConfiguration input)
    {
        var materials = input.Fabrics.Select((x, index) => new SewingPricingMaterialRow(index + 1, "Fabric:" + x.ReferenceId,
                x.Quantity, x.Unit, true, false, true, true))
            .Concat(input.Accessories.Select((x, index) => new SewingPricingMaterialRow(index + 41, "Accessory:" + x.ReferenceId,
                x.Quantity, x.Unit, false, false, true, true))).ToArray();
        var operations = input.Operations.Select((x, index) => new SewingPricingOperationRow(index + 1, x.ReferenceId,
            x.Quantity, x.ManualMinutes, true)).ToArray();
        return new(materials, operations, new(input.Delivery, input.Packaging,
            input.Applications.Sum(x => x.Cost), input.Applications.Sum(x => x.SaleAmount)));
    }
}
