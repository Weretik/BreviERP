using Domain.Reference.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Reference.Congigurations;

public static class ReferenceConverters
{
    public static readonly ValueConverter<Percent, decimal> PercentConvert =
        new(
            p => p.Value,
            v => Percent.From(v)
        );

    public static readonly ValueConverter<AdditionalReferenceId, int> AdditionalReferenceIdConvert =
        new(
            id => id.Value,
            v => AdditionalReferenceId.From(v)
        );

    public static readonly ValueConverter<Money, decimal> MoneyConvert =
        new(
            m => m.Amount,
            v => new Money(v, "UAH")
        );
}
