namespace Accounting.Domain.Errors;

public static class TransactionCategoryErrors
{
    public static AccountingDomainError IdMustBePositive() =>
        new("Accounting.TransactionCategory.Id.Invalid",
            "Transaction category id must be greater than zero");

    public static AccountingDomainError NameIsRequired() =>
        new("Accounting.TransactionCategory.Name.Required",
            "Transaction category name is required");

    public static AccountingDomainError NameLengthInvalid(int length) =>
        new("Accounting.TransactionCategory.Name.LengthInvalid",
            $"Transaction category name length must be between 1 and 100 characters. Actual:{length}");

    public static AccountingDomainError PathIsRequired() =>
        new("Accounting.TransactionCategory.Path.Required",
            "Transaction category path is required");

    public static AccountingDomainError CannotSetSelfAsParent(string path) =>
        new("Accounting.TransactionCategory.Reparent.SelfParentForbidden",
            $"You cannot set a transaction category as its own parent. Actual:{path}");

    public static AccountingDomainError CannotMoveUnderDescendant(string currentPath, string newParentPath) =>
        new("Accounting.TransactionCategory.Reparent.CycleForbidden",
            $"You cannot move a transaction category under its own descendant (cycle in the tree). Actual:{currentPath}, {newParentPath}");
}
