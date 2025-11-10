using Domain.Accounting.Enumerations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Accounting.Convertors;

public class EnumConverter
{
    public static readonly ValueConverter<TransactionType, string> PriceTypeConvert =
        new(
            priceType => priceType.Name,
            name => TransactionType.FromName(name, false)
        );
}
