namespace Accounting.Domain.Accounts;

[ValueObject<Guid>]
public readonly partial struct FinanceAccountId
{
    public static FinanceAccountId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Account id must not be empty.") : Validation.Ok;
}
