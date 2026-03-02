using Accounting.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Accounting.Infrastructure.Convertors;

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
