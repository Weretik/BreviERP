using BuildingBlocks.Domain.Abstractions;
using Domain.Accounting.ValueObjects;

namespace Domain.Accounting.Entities;

public sealed class TransactionCategory : BaseCategory<TransactionCategoryId, TransactionCategory>, IAggregateRoot
{
    #region Constructors
    private TransactionCategory() { }
    private TransactionCategory(TransactionCategoryId id, string name, CategoryPath path)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
    }
    #endregion

    #region Factories
    public static TransactionCategory Create(TransactionCategoryId id, string name, CategoryPath path)
        => new(id, name, path);
    public static TransactionCategory CreateRoot(TransactionCategoryId id, string name)
        => new(id, name, CategoryPath.Root(id.ToString()));
    public static TransactionCategory CreateChild(TransactionCategoryId id, string name, TransactionCategory parent)
        => new(id, name, parent.Path.Append(id.ToString()));
    #endregion
}
