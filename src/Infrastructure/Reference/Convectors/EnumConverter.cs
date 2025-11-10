using Domain.Reference.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Reference.Convectors;

public class EnumConverter
{
    public static readonly ValueConverter<Percent, decimal> PriceTypeConvert =
        new(
            ratio => ratio.Value,
            value => Percent.From(value)
        );
}
