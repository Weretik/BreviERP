namespace Accounting.Domain.Statements.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct BankStatementLineId
{
    public static BankStatementLineId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Statement line id must not be empty.") : Validation.Ok;
}
