namespace Accounting.Domain.Transactions;

[ValueObject<Guid>]
public readonly partial struct FinanceTransactionId
{
    public static FinanceTransactionId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Transaction id must not be empty.") : Validation.Ok;
}
