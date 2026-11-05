namespace Accounting.Domain.Categories;

[ValueObject<Guid>]
public readonly partial struct FinanceCategoryId
{
    public static FinanceCategoryId New() => From(Guid.CreateVersion7());

    private static Validation Validate(Guid value) =>
        value == Guid.Empty ? Validation.Invalid("Category id must not be empty.") : Validation.Ok;
}
