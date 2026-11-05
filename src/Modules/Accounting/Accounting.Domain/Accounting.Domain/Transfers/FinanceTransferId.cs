namespace Accounting.Domain.Transfers;

[ValueObject<Guid>]
public readonly partial struct FinanceTransferId
{
    public static FinanceTransferId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Transfer id must not be empty.") : Validation.Ok;
}
