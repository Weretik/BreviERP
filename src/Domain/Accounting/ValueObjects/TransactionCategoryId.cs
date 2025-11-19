using Domain.Common.Helpers;

namespace Domain.Accounting.ValueObjects;

[ValueObject<int>]
public partial struct TransactionCategoryId
{
    private static Validation Validate(int value)
        => IntIdValidator.ValidateIntId(value, nameof(TransactionCategoryId));
}
