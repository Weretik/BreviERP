namespace Domain.Accounting.Entities;

public sealed class TransactionCategory : CategoryNode<TransactionCategoryId, TransactionCategory>, IAggregateRoot
{
    private TransactionCategory() { }
    private TransactionCategory(TransactionCategoryId id, string name, LTree path, DateTime createdDate, TransactionCategoryId? parentId)
    {
        Id = Guard.Against.Default(id, nameof(id));
        SetName(name);
        SetParent(parentId);
        SetPath(path);
        MarkAsCreated(createdDate);
    }

    public static TransactionCategory Create(TransactionCategoryId id, string name, LTree path, DateTime createdAt,
        TransactionCategoryId? parentId = null)
        => new(id, name, path, createdAt, parentId);

    public static TransactionCategory CreateRoot(TransactionCategoryId id, string name, DateTime createdAt)
        => new(
            id: id,
            name: name,
            path: (LTree)$"root.n{id}",
            createdDate: createdAt,
            parentId: null);

    public static TransactionCategory CreateChild(TransactionCategoryId id, string name, DateTime createdAt,
        TransactionCategory parent)
        => new(
            id: id,
            name: name,
            path: parent.Path + (LTree)$".n{id}",
            createdDate: createdAt,
            parentId: parent.Id);
}
