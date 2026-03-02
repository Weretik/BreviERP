using Accounting.Domain.Enumerations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Accounting.Infrastructure.Convertors;

public class EnumConverter
{
    public static readonly ValueConverter<TransactionType, string> PriceTypeConvert =
        new(
            priceType => priceType.Name,
            name => TransactionType.FromName(name, false)
        );
}
