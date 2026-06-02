using Domain.Reference.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Reference.Domain.ValueObjects;

namespace Reference.Infrastructure.Converters;

public static class ReferenceConverters
{
    public static readonly ValueConverter<AdditionalReferenceId, int> AdditionalReferenceIdConvert =
        new(
            id => id.Value,
            v => AdditionalReferenceId.From(v)
        );
    public static readonly ValueConverter<FabricId, int> FabricIdConvert =
        new(
            id => id.Value,
            v => FabricId.From(v)
        );

    public static readonly ValueConverter<GarmentPartId, int> GarmentPartIdConvert =
        new(
            id => id.Value,
            v => GarmentPartId.From(v)
        );

    public static readonly ValueConverter<GarmentAccessoryId, int> GarmentAccessoryIdConvert =
        new(
            id => id.Value,
            v => GarmentAccessoryId.From(v)
        );

    public static readonly ValueConverter<GarmentPartOperationId, int> GarmentPartOperationIdConvert =
        new(
            id => id.Value,
            v => GarmentPartOperationId.From(v)
        );

    public static readonly ValueConverter<SupplierId, int> SupplierIdConvert =
        new(
            id => id.Value,
            v => SupplierId.From(v)
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

