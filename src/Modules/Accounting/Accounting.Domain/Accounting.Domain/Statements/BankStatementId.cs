namespace Accounting.Domain.Statements;

[ValueObject<Guid>]
public readonly partial struct BankStatementId
{
    public static BankStatementId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Statement id must not be empty.") : Validation.Ok;
}
