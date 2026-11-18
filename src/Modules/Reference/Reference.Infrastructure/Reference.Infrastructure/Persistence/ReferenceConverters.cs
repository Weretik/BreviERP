using Reference.Domain.GarmentPartOperations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Reference.Domain.FabricColors;
using Reference.Domain.PpeMarkupRules;
using Reference.Domain.ProductSections;
using Reference.Domain.PricingParameters;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.Suppliers;
using Reference.Domain.Fabrics;
using Reference.Domain.GarmentParts;
using BuildingBlocks.Domain;

namespace Reference.Infrastructure.Persistence;

public static class ReferenceConverters
{
    public static readonly ValueConverter<PricingParameterId, int> PricingParameterIdConvert =
        new(
            id => id.Value,
            v => PricingParameterId.From(v)
        );
    public static readonly ValueConverter<PpeMarkupRuleId, int> PpeMarkupRuleIdConvert =
        new(
            id => id.Value,
            v => PpeMarkupRuleId.From(v)
        );
    public static readonly ValueConverter<FabricColorId, int> FabricColorIdConvert =
        new(
            id => id.Value,
            v => FabricColorId.From(v)
        );
    public static readonly ValueConverter<GarmentPartId, int> GarmentPartIdConvert =
        new(
            id => id.Value,
            v => GarmentPartId.From(v)
        );

    public static readonly ValueConverter<Percent, decimal> PercentConvert =
        new(
            p => p.Value,
            v => Percent.From(v)
        );


    public static readonly ValueConverter<MoneyAmount, decimal> MoneyAmountConvert =
        new(
            m => m.Value,
            v => MoneyAmount.From(v)
        );
}