using Domain.Accounting.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Accounting.Convertors;

public class IdConverter
{
    public static readonly ValueConverter<TransactionId, int> TransactionIdConvert =
        new(
            id => id.Value,
            value => TransactionId.From(value)
        );
    public static readonly ValueConverter<TransactionCategoryId, int> TransactionCategoryIdConvert =
        new(
            id => id.Value,
            value => TransactionCategoryId.From(value)
        );
}
