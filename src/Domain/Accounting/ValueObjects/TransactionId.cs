using Domain.Common.Helpers;

namespace Domain.Accounting.ValueObjects;

[ValueObject<int>]
public partial struct TransactionId
{
    private static Validation Validate(int value)
        => IntIdValidator.ValidateIntId(value, nameof(TransactionId));
}
