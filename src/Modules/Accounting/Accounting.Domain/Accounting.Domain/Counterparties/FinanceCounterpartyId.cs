namespace Accounting.Domain.Counterparties;

[ValueObject<Guid>]
public readonly partial struct FinanceCounterpartyId
{
    public static FinanceCounterpartyId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Counterparty id must not be empty.") : Validation.Ok;
}
